using LedgerService.Domain.Entities;

namespace LedgerService.Domain.Abstractions;

public interface ILedgerRepository
{
    Task<LedgerEntry?> GetLatestEntryForAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<LedgerEntry> Entries, int TotalCount)> GetEntriesForAccountAsync(
        Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LedgerEntry>> GetEntriesForPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<decimal> GetLatestBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task AddEntriesAndMarkProcessedAsync(
        LedgerEntry debitEntry,
        LedgerEntry creditEntry,
        Guid eventId,
        string eventType,
        CancellationToken cancellationToken = default);
}
