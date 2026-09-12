using PaymentGateway.Contracts;

namespace LedgerService.Infrastructure.EventHandler;

public interface ILedgerEventHandler
{
    Task HandlePaymentSucceededAsync(PaymentSucceededEvent evt, CancellationToken cancellationToken = default);
}