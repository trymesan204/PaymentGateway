using LedgerService.Domain.Abstractions;
using LedgerService.Domain.Entities;
using LedgerService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly LedgerDbContext _context;

    public LedgerRepository(LedgerDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerEntry?> GetLatestEntryForAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries
            .AsNoTracking()
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<LedgerEntry> Entries, int TotalCount)> GetEntriesForAccountAsync(
        Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.LedgerEntries
            .AsNoTracking()
            .Where(e => e.AccountId == accountId);

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, totalCount);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetEntriesForPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries
            .AsNoTracking()
            .Where(e => e.PaymentId == paymentId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
