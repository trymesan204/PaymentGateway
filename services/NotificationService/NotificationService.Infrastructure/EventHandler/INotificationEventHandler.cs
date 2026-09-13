using PaymentGateway.Contracts;

namespace NotificationService.Infrastructure.EventHandler;

public interface INotificationEventHandler
{
    Task HandlePaymentSucceededAsync(PaymentSucceededEvent evt, CancellationToken cancellationToken = default);
}
