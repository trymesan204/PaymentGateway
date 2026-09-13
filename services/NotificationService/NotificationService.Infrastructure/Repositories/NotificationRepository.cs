using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Context;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationLog>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .AsNoTracking()
            .Where(n => n.PaymentId == paymentId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(e => e.EventId == eventId, cancellationToken);
    }

    public async Task AddAndMarkProcessedAsync(
        NotificationLog notificationLog,
        Guid eventId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        await _context.NotificationLogs.AddAsync(notificationLog, cancellationToken);
        await _context.ProcessedEvents.AddAsync(new ProcessedEvent
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
