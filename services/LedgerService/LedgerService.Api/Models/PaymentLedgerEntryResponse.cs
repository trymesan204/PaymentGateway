using LedgerService.Domain.Enums;

namespace LedgerService.Api.Models;

public class PaymentLedgerEntryResponse
{
    public Guid EntryId { get; set; }
    public Guid AccountId { get; set; }
    public LedgerEntryType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
}
