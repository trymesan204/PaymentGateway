using PaymentService.Models;

namespace PaymentService.Abstractions;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default);
}