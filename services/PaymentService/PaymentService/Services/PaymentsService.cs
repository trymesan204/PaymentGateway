using PaymentGateway.Contracts;
using PaymentService.Abstractions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Models;
using System.Text.Json;

namespace PaymentService.Services;

public class PaymentsService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ILedgerClient _ledgerClient;
    private readonly ILogger<PaymentsService> _logger;

    public PaymentsService(
        IPaymentRepository paymentRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        IPaymentProcessor paymentProcessor,
        ILedgerClient ledgerClient,
        ILogger<PaymentsService> logger)
    {
        _paymentRepository = paymentRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _paymentProcessor = paymentProcessor;
        _ledgerClient = ledgerClient;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existingPayment != null)
        {
            _logger.LogInformation(
                "Duplicate payment request for idempotency key {IdempotencyKey}; returning existing payment {PaymentId}",
                request.IdempotencyKey,
                existingPayment.Id);
            return new PaymentResult() { PaymentResponse = MapToResponse(existingPayment), IsNew = false };
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = request.IdempotencyKey,
            Type = request.Type,
            PayerId = request.PayerId,
            PayeeId = request.PayeeId,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            PaymentMethod = request.PaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        if (request.Type is Domain.Enums.PaymentType.Transfer or Domain.Enums.PaymentType.MerchantPayment)
        {
            var balance = await _ledgerClient.GetBalanceAsync(request.PayerId!.Value, cancellationToken);
            if (balance < request.Amount)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Insufficient funds";
                await _paymentRepository.AddAsync(payment, cancellationToken);
                return new PaymentResult { PaymentResponse = MapToResponse(payment), IsNew = true };
            }
        }

        await _paymentRepository.AddAsync(payment, cancellationToken);

        PaymentStatus status;
        try
        {
            status = await _paymentProcessor.ProcessPaymentAsync(payment.Amount, payment.Currency, cancellationToken);
            payment.Status = status;
        }
        catch (TimeoutException ex)
        {
            payment.Status = PaymentStatus.Pending;
            _logger.LogWarning(
                ex,
                "Payment processor timed out for payment {PaymentId}; status set to {Status}",
                payment.Id,
                payment.Status);
        }

        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        if (payment.Status == PaymentStatus.Succeeded)
        {
            var evt = new PaymentSucceededEvent(
                EventId: Guid.NewGuid(),
                PaymentId: payment.Id,
                Type: ToContractPaymentType(payment.Type),
                PayerId: payment.PayerId,
                PayeeId: payment.PayeeId,
                Amount: payment.Amount,
                Currency: payment.Currency,
                OccurredAt: DateTime.UtcNow
            );

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(PaymentSucceededEvent),
                Payload = JsonSerializer.Serialize(evt),
                Published = false,
                CreatedAt = DateTime.UtcNow
            };

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken); // ONE transaction: payment update and outbox insert commit together, or neither does

        _logger.LogInformation(
            "Processed payment {PaymentId} with status {Status} for payee {PayeeId}",
            payment.Id,
            payment.Status,
            payment.PayeeId);

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
        Type = payment.Type,
        PayerId = payment.PayerId,
        PayeeId = payment.PayeeId,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        CreatedAt = payment.CreatedAt,
        UpdatedAt = payment.UpdatedAt
    };

    private static PaymentGateway.Contracts.PaymentType ToContractPaymentType(PaymentService.Domain.Enums.PaymentType type) => type switch
    {
        PaymentService.Domain.Enums.PaymentType.TopUp => PaymentGateway.Contracts.PaymentType.TopUp,
        PaymentService.Domain.Enums.PaymentType.Transfer => PaymentGateway.Contracts.PaymentType.Transfer,
        PaymentService.Domain.Enums.PaymentType.MerchantPayment => PaymentGateway.Contracts.PaymentType.MerchantPayment,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown payment type")
    };
}
