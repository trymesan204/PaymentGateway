namespace LedgerService.Api.Models;

public class SeedBalanceRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
