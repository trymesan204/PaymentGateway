using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models;

public class CreatePaymentRequest
{
    [Required]
    public Guid IdempotencyKey { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public string? PaymentMethod { get; set; }
}
