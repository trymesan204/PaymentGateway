using PaymentService.Abstractions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services;

public class PaymentsService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProcessor _paymentProcessor;

    public PaymentsService(IPaymentRepository paymentRepository, IPaymentProcessor paymentProcessor)
    {
        _paymentRepository = paymentRepository;
        _paymentProcessor = paymentProcessor;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existingPayment != null) return new PaymentResult() { PaymentResponse = MapToResponse(existingPayment), IsNew = false};
            
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            IdempotencyKey = request.IdempotencyKey,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            PaymentMethod = request.PaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        PaymentStatus status;
        try
        {
            status = await _paymentProcessor.ProcessPaymentAsync(payment.Amount, payment.Currency, cancellationToken);
            payment.Status = status;
        }
        catch (TimeoutException e)
        {
            payment.Status = PaymentStatus.Pending;
        }

        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return new PaymentResult() { PaymentResponse = MapToResponse(payment), IsNew = true };
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
        UpdatedAt = payment.UpdatedAt
    };
}
