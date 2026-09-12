namespace PaymentService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; }       // "PaymentSucceeded" — lets you support more event types later
    public string Payload { get; set; }          // the serialized PaymentSucceededEvent, as JSON
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}