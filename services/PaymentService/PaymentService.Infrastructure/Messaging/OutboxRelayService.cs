using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Messaging;

public class OutboxRelayService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxRelayService> _logger;

    public OutboxRelayService(IServiceScopeFactory scopeFactory, ILogger<OutboxRelayService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var unpublished = await outboxRepository.GetUnpublishedAsync(batchSize: 20, stoppingToken);

            foreach (var message in unpublished)
            {
                try
                {
                    await publisher.PublishAsync(message.EventType, message.Payload, stoppingToken);
                    message.Published = true;
                    message.PublishedAt = DateTime.UtcNow;
                    await outboxRepository.UpdateAsync(message, stoppingToken);
                    _logger.LogInformation("Published outbox message {MessageId} ({EventType})", message.Id, message.EventType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {MessageId}, will retry next poll", message.Id);
                    // deliberately don't mark as published — next poll picks it up again
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // polling interval
        }
    }
}