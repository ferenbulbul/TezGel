

// TezGel.ReservationConsumer/ReservationConsumerWorker.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // IServiceScopeFactory için
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TezGel.Application.Interfaces.Repositories; // IActionRepository
using TezGel.Domain.Enums;
using TezGel.Application.DTOs.RabbitMq; // ActionStatus

namespace TezGel.ReservationConsumer
{
    public class ReservationConsumerWorker : BackgroundService
    {
        private readonly ILogger<ReservationConsumerWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory; // Repository'leri scope içinde çözmek için
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        public ReservationConsumerWorker(
            ILogger<ReservationConsumerWorker> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory) // DI ile gelecek
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var rabbitMqConfig = _configuration.GetSection("RabbitMq");
            _queueName = rabbitMqConfig["ProcessingQueueName"];

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqConfig["HostName"],
                UserName = rabbitMqConfig["UserName"],
                Password = rabbitMqConfig["Password"],
                DispatchConsumersAsync = true // Asenkron consumer için
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Consumer sadece dinleyeceği kuyruğun varlığından emin olmalı.
                // Exchange ve diğer kuyrukları declare etmesine gerek yok, onlar Publisher (API) tarafında yapıldı.
                _channel.QueueDeclare(queue: _queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                _logger.LogInformation($"RabbitMQ Consumer initialized. Listening to queue: '{_queueName}'");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Cannot connect to RabbitMQ. Worker will not start properly.");
                // Bu durumda worker'ın durması veya periyodik olarak yeniden bağlanmayı denemesi gerekebilir.
                // Şimdilik sadece logluyoruz, uygulama durabilir.
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not initialized. Consumer cannot start.");
                return Task.CompletedTask;
            }

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                ReservationTimeoutMessage reservationMessage = null;

                _logger.LogInformation($"Received raw message: {messageJson}");

                try
                {
                    reservationMessage = JsonSerializer.Deserialize<ReservationTimeoutMessage>(messageJson);
                    if (reservationMessage == null || reservationMessage.ReservationId == Guid.Empty)
                    {
                        _logger.LogWarning("Received message could not be deserialized properly or ReservationId is missing. Skipping.");
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Mesajı onaylayıp geç
                        return;
                    }

                    _logger.LogInformation($"Processing expired reservation for ReservationId: {reservationMessage.ReservationId}");

                    // Repository'leri ve DbContext'i her mesaj için yeni bir scope'ta çözmeliyiz
                    // Bu, DbContext'in thread güvenliği ve lifecycle yönetimi için önemlidir.
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var actionRepository = scope.ServiceProvider.GetRequiredService<IActionRepository>();
                        // Eğer DbContext'e doğrudan ihtiyacınız varsa onu da çözebilirsiniz:
                        // var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();

                        var reservation = await actionRepository.GetByIdAsync(reservationMessage.ReservationId);

                        if (reservation == null)
                        {
                            _logger.LogWarning($"Reservation with Id {reservationMessage.ReservationId} not found in DB. Acknowledging message.");
                        }
                        else if (reservation.Status == ActionStatus.Pending)
                        {
                            _logger.LogInformation($"Reservation {reservation.Id} is Pending. Setting to Expired.");
                            reservation.Status = ActionStatus.Expired;
                            // reservation.UpdatedAt = DateTime.UtcNow; // Gerekirse
                            await actionRepository.UpdateAsync(reservation);
                            _logger.LogInformation($"Reservation {reservation.Id} status updated to Expired.");

                            // İSTEĞE BAĞLI: Redis lock'unu da serbest bırakabilirsiniz, ama zaten TTL'i dolmuş olmalı.
                            // var lockService = scope.ServiceProvider.GetRequiredService<ILockService>();
                            // await lockService.ReleaseLockAsync($"product:lock:{reservation.ProductId}");
                        }
                        else
                        {
                            _logger.LogInformation($"Reservation {reservation.Id} status is '{reservation.Status}'. No action taken.");
                        }
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Mesajı işledik, onaylıyoruz.
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, $"Failed to deserialize message: {messageJson}. Acknowledging to remove from queue.");
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Bozuk mesajı kuyruktan çıkar
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing message for ReservationId: {reservationMessage?.ReservationId}. Nacking message (requeue=false).");
                    // Hata oldu, mesajı tekrar kuyruğa sokmuyoruz (requeue=false) ki sonsuz döngüye girmesin.
                    // İdealde, bu tür mesajlar bir "error" veya "dead-letter" kuyruğuna yönlendirilmeli.
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            // Prefetch count: Consumer'ın aynı anda RabbitMQ'dan kaç mesaj alacağını belirler.
            // Bu, yükü consumer'lar arasında daha dengeli dağıtmaya yardımcı olabilir.
            // Eğer bir mesajın işlenmesi uzun sürüyorsa düşük bir değer (örn: 1) iyi olabilir.
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false, // Manuel acknowledgement
                                 consumer: consumer);

            _logger.LogInformation("Consumer started. Waiting for messages on queue '{QueueName}'. Press Ctrl+C to exit.", _queueName);
            return Task.CompletedTask; // BackgroundService için ExecuteAsync'in uzun süre çalışması beklenir.
                                       // BasicConsume zaten kendi thread'lerinde dinlemeye başlar.
        }

        public override void Dispose()
        {
            _logger.LogInformation("RabbitMQ Consumer is disposing.");
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
