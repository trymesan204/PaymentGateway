using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

public class MockPaymentProcessor : IPaymentProcessor
{
    public Task<PaymentStatus> ProcessPaymentAsync(decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        int[] failed = [5, 10, 15];
        int timeout = 0;
        var randomStatus = Random.Shared.Next(20);
        var status = randomStatus == timeout 
        ? PaymentStatus.Timeout 
        : (failed.Contains(randomStatus)
            ? PaymentStatus.Failed
            : PaymentStatus.Succeeded
        );

        return Task.FromResult(status);
    }
}
