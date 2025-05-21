// WebAPI_Project/Infrastructure/RabbitMqInitializerService.cs (veya Extensions klasöründe)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqInitializerService : IHostedService
{
    private readonly ILogger<RabbitMqInitializerService> _logger;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMqInitializerService(
        ILogger<RabbitMqInitializerService> logger,
        IConnectionFactory connectionFactory, // DI'dan gelecek
        IConfiguration configuration)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Initializer Service is starting.");
        try
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            var rabbitMqConfig = _configuration.GetSection("RabbitMQ"); // appsettings'den okuyacağız

            // 1. Ana Exchange (mesajların ilk gönderileceği yer)
            _channel.ExchangeDeclare(exchange: rabbitMqConfig["WaitExchangeName"], type: ExchangeType.Direct, durable: true);
            _logger.LogInformation($"Exchange '{rabbitMqConfig["WaitExchangeName"]}' declared.");

            // 2. Dead Letter Exchange (DLX)
            _channel.ExchangeDeclare(exchange: rabbitMqConfig["ProcessingExchangeName"], type: ExchangeType.Direct, durable: true);
            _logger.LogInformation($"Exchange '{rabbitMqConfig["ProcessingExchangeName"]}' declared.");

            // 3. Bekleme Kuyruğu (mesajların TTL ile bekleyeceği kuyruk)
            var waitQueueArgs = new Dictionary<string, object>
            {
                { "x-message-ttl", int.Parse(rabbitMqConfig["MessageTTL"]) },
                { "x-dead-letter-exchange", rabbitMqConfig["ProcessingExchangeName"] },
                { "x-dead-letter-routing-key", rabbitMqConfig["ProcessingRoutingKey"] }
            };
            _channel.QueueDeclare(queue: rabbitMqConfig["WaitQueueName"],
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: waitQueueArgs);
            _channel.QueueBind(queue: rabbitMqConfig["WaitQueueName"],
                              exchange: rabbitMqConfig["WaitExchangeName"],
                              routingKey: rabbitMqConfig["WaitRoutingKey"]);
            _logger.LogInformation($"Queue '{rabbitMqConfig["WaitQueueName"]}' declared and bound to '{rabbitMqConfig["WaitExchangeName"]}'.");

            // 4. İşleme Kuyruğu (süresi dolan mesajların DLX tarafından yönlendirileceği son kuyruk)
            _channel.QueueDeclare(queue: rabbitMqConfig["ProcessingQueueName"],
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueBind(queue: rabbitMqConfig["ProcessingQueueName"],
                              exchange: rabbitMqConfig["ProcessingExchangeName"],
                              routingKey: rabbitMqConfig["ProcessingRoutingKey"]);
            _logger.LogInformation($"Queue '{rabbitMqConfig["ProcessingQueueName"]}' declared and bound to '{rabbitMqConfig["ProcessingExchangeName"]}'.");

            _logger.LogInformation("RabbitMQ topology (exchanges, queues, bindings) initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing RabbitMQ topology.");
            // Uygulamanın başlamasını engelleyebilir veya sadece loglayıp devam edebilir, kararı size kalmış.
            // throw; // Eğer kritikse ve uygulama başlamamalıysa
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Initializer Service is stopping.");
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ connection closed by Initializer Service.");
        return Task.CompletedTask;
    }
}