namespace PaymentGateway.Contracts;
public record PaymentSucceededEvent(
    Guid EventId,          // unique per event instance — this is what Ledger's ProcessedEvents table keys on
    Guid PaymentId,
    PaymentType Type,
    Guid? PayerId,
    Guid PayeeId,
    decimal Amount,
    string Currency,
    DateTime OccurredAt
);

public enum PaymentType
{
    TopUp,
    Transfer,
    MerchantPayment
}