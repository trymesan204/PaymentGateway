using LedgerService.Domain.Enums;

namespace LedgerService.Domain.Entities;

public class LedgerEntry
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }      // links the two entries from the same payment together
    public Guid AccountId { get; set; }      // could be a UserId, MerchantId, or the System account's fixed Guid
    public LedgerEntryType Type { get; set; } // Debit or Credit
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }
}
