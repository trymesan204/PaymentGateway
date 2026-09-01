namespace LedgerService.Api.Models;

public class AccountEntriesResponse
{
    public Guid AccountId { get; set; }
    public List<AccountLedgerEntryResponse> Entries { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
