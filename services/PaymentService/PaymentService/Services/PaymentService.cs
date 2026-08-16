using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default);
}

public class PaymentServiceImpl : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProcessor _paymentProcessor;

    public PaymentServiceImpl(IPaymentRepository paymentRepository, IPaymentProcessor paymentProcessor)
    {
        _paymentRepository = paymentRepository;
        _paymentProcessor = paymentProcessor;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var status = await _paymentProcessor.ProcessPaymentAsync(payment.Amount, payment.Currency, cancellationToken);

        payment.Status = status;
        payment.ProcessedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return MapToResponse(payment);
    }

    public async Task<PaymentResponse?> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        return payment is null ? null : MapToResponse(payment);
    }

    private static PaymentResponse MapToResponse(Payment payment) => new()
    {
        Id = payment.Id,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt
    };
}
