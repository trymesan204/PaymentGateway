namespace LedgerService.Api.Models;

public class AccountBalanceResponse
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime AsOf { get; set; }
}
