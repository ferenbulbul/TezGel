// TezGel.Infrastructure/Messaging/RabbitMqPublisher.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using TezGel.Application.DTOs.RabbitMq;
using TezGel.Application.Interfaces;
using TezGel.Application.Interfaces.Services; // IMessagePublisher gibi bir interface tanımlayabiliriz


namespace TezGel.Infrastructure.Messaging
{
    // Application katmanında bu interface'i tanımlayıp burada implemente edebiliriz:
    // public interface IMessagePublisher
    // {
    //    void PublishReservationTimeoutMessage(ReservationTimeoutMessage message);
    // }
    // Şimdilik doğrudan implementasyon yapalım, sonra interface ekleyebiliriz.

    public class RabbitMqPublisher  : IMessagePublisher
    {
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;

        public RabbitMqPublisher(
            ILogger<RabbitMqPublisher> logger,
            IConnectionFactory connectionFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
        }

        public void PublishReservationTimeoutMessage(ReservationTimeoutMessage message)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var rabbitMqConfig = _configuration.GetSection("RabbitMQ");
                    var exchangeName = rabbitMqConfig["WaitExchangeName"];
                    var routingKey = rabbitMqConfig["WaitRoutingKey"];

                    // Exchange ve kuyrukların RabbitMqInitializerService tarafından oluşturulduğunu varsayıyoruz.
                    // İsteğe bağlı olarak burada da idempotent bir şekilde declare edilebilirler ama genellikle gerekmez.
                    // channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

                    var messageBody = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(messageBody);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Mesajın kalıcı olmasını sağlar (RabbitMQ çökse bile kaybolmaz)

                    channel.BasicPublish(
                        exchange: exchangeName,
                        routingKey: routingKey,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation("Successfully published ReservationTimeoutMessage. ActivationId: {ActivationId}, ProductId: {ProductId}",
                        message.ReservationId, message.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ReservationTimeoutMessage. ActivationId: {ActivationId}, ProductId: {ProductId}",
                    message.ReservationId, message.ProductId);
                // Burada hata yönetimi önemli: Tekrar deneme, hata kuyruğuna atma vs. düşünülebilir.
                // Şimdilik sadece logluyoruz.
                throw; // Çağıran katmanın hatadan haberdar olması için
            }
        }
    }
}