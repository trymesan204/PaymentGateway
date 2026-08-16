using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

public class MockPaymentProcessor : IPaymentProcessor
{
    public Task<PaymentStatus> ProcessPaymentAsync(decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        var status = Random.Shared.Next(2) == 0
            ? PaymentStatus.Succeeded
            : PaymentStatus.Failed;

        return Task.FromResult(status);
    }
}
