using System.ComponentModel.DataAnnotations;
using PaymentService.Domain.Enums;

namespace PaymentService.Models;

public class CreatePaymentRequest
{
    [Required]
    public Guid IdempotencyKey { get; set; }

    [Required]
    public PaymentType Type { get; set; }

    public Guid? PayerId { get; set; }             // not required for TopUp — funds originate outside the ledger

    [Required]
    public Guid PayeeId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public string? PaymentMethod { get; set; }
}
