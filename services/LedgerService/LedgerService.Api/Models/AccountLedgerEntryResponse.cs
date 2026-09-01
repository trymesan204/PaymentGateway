using LedgerService.Domain.Enums;

namespace LedgerService.Api.Models;

public class AccountLedgerEntryResponse
{
    public Guid EntryId { get; set; }
    public Guid PaymentId { get; set; }
    public LedgerEntryType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }
}
