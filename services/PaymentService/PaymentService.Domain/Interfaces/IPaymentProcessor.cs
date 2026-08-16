using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

public interface IPaymentProcessor
{
    Task<PaymentStatus> ProcessPaymentAsync(decimal amount, string currency, CancellationToken cancellationToken = default);
}
