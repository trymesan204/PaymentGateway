namespace NotificationService.Domain.Entities;

public class ProcessedEvent
{
    public Guid EventId { get; set; }      // same idempotency pattern as Ledger
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }
}
