namespace LedgerService.Domain.Entities;

public class ProcessedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }
}
