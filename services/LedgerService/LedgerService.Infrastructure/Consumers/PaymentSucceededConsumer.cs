using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using PaymentGateway.Contracts;
using System.Text.Json;
using LedgerService.Infrastructure.EventHandler;

public class PaymentSucceededConsumer : BackgroundService
{
    private const string ExchangeName = "payment.events";
    private const string QueueName = "ledger.payment-events";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentSucceededConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public PaymentSucceededConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PaymentSucceededConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: string.Empty);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            var eventType = args.BasicProperties.Type;
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                if (eventType == nameof(PaymentSucceededEvent))
                {
                    var evt = JsonSerializer.Deserialize<PaymentSucceededEvent>(json)
                        ?? throw new InvalidOperationException("Failed to deserialize PaymentSucceededEvent");

                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<ILedgerEventHandler>();
                    await handler.HandlePaymentSucceededAsync(evt, stoppingToken);
                }

                _channel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Entry failed");

                var retryCount = GetRetryCount(args.BasicProperties) + 1;
                if (args.Redelivered && retryCount >= 3)
                {
                    _logger.LogError(ex, "Message failed after {RetryCount} attempts, sending to DLQ", retryCount);
                    _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false); // false = don't requeue, goes to DLQ instead
                }
                else
                {
                    _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
                }
            }
        };
        _logger.LogError( "Entry added");
        _channel.BasicConsume(QueueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private int GetRetryCount(IBasicProperties properties)
    {
        if (properties.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var value))
        {
            return Convert.ToInt32(value);
        }
        return 0;
    }
}