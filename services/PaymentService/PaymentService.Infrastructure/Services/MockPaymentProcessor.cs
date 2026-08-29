using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

public class MockPaymentProcessor : IPaymentProcessor
{
    public async Task<PaymentStatus> ProcessPaymentAsync(decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        await Task.Delay(Random.Shared.Next(50, 300), cancellationToken);

        var roll = Random.Shared.Next(100);
        return roll switch
        {
            < 5 => throw new TimeoutException("Provider did not respond in time"),
            < 20 => PaymentStatus.Failed,
            _ => PaymentStatus.Succeeded
        };
    }
}
