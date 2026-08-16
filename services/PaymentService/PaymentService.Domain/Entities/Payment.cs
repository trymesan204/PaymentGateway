using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid IdempotencyKey { get; set; }      // client-supplied, prevents double charging
    public decimal Amount { get; set; }
    public string Currency { get; set; }           // "NPR", "USD" etc.
    public PaymentStatus Status { get; set; }
    public string? PaymentMethod { get; set; }      // "card", "wallet" — just metadata for the mock
    public string? FailureReason { get; set; }      // populated only if Status = Failed
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
