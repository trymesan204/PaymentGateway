namespace NotificationService.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid RecipientAccountId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public NotificationStatus Status { get; set; }   // Sent, Failed
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum NotificationStatus
{
    Sent,
    Failed
}