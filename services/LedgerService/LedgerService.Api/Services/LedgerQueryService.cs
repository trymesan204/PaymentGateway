using LedgerService.Api.Abstractions;
using LedgerService.Api.Models;
using LedgerService.Domain.Abstractions;
using LedgerService.Domain.Entities;

namespace LedgerService.Api.Services;

public class LedgerQueryService : ILedgerQueryService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ILogger<LedgerQueryService> _logger;

    public LedgerQueryService(ILedgerRepository ledgerRepository, ILogger<LedgerQueryService> logger)
    {
        _ledgerRepository = ledgerRepository;
        _logger = logger;
    }

    public async Task<AccountBalanceResponse?> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var latestEntry = await _ledgerRepository.GetLatestEntryForAccountAsync(accountId, cancellationToken);

        if (latestEntry is null)
        {
            _logger.LogInformation("No ledger entries found for account {AccountId}", accountId);
            return null;
        }

        return new AccountBalanceResponse
        {
            AccountId = accountId,
            Balance = latestEntry.BalanceAfter,
            Currency = latestEntry.Currency,
            AsOf = DateTime.Now
        };
    }

    public async Task<AccountEntriesResponse> GetAccountEntriesAsync(
        Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (entries, totalCount) = await _ledgerRepository.GetEntriesForAccountAsync(accountId, page, pageSize, cancellationToken);

        return new AccountEntriesResponse
        {
            AccountId = accountId,
            Entries = entries.Select(MapToAccountEntryResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<PaymentLedgerEntryResponse>> GetPaymentEntriesAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var entries = await _ledgerRepository.GetEntriesForPaymentAsync(paymentId, cancellationToken);
        return entries.Select(MapToPaymentEntryResponse).ToList();
    }

    private static AccountLedgerEntryResponse MapToAccountEntryResponse(LedgerEntry entry) => new()
    {
        EntryId = entry.Id,
        PaymentId = entry.PaymentId,
        Type = entry.Type,
        Amount = entry.Amount,
        BalanceAfter = entry.BalanceAfter,
        CreatedAt = entry.CreatedAt
    };

    private static PaymentLedgerEntryResponse MapToPaymentEntryResponse(LedgerEntry entry) => new()
    {
        EntryId = entry.Id,
        AccountId = entry.AccountId,
        Type = entry.Type,
        Amount = entry.Amount,
        BalanceAfter = entry.BalanceAfter
    };
}
