using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions;

public interface INotificationRepository
{
    Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationLog>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task AddLogsAndMarkProcessedAsync(
        IReadOnlyList<NotificationLog> notificationLogs,
        Guid eventId,
        string eventType,
        CancellationToken cancellationToken = default);
}
