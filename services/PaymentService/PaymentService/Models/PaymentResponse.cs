using PaymentService.Domain.Enums;

namespace PaymentService.Models;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public PaymentType Type { get; set; }
    public Guid? PayerId { get; set; }
    public Guid PayeeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
