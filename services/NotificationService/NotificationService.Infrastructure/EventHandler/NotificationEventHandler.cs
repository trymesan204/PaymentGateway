using Microsoft.Extensions.Logging;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using PaymentGateway.Contracts;

namespace NotificationService.Infrastructure.EventHandler;

public class NotificationEventHandler : INotificationEventHandler
{
    private readonly INotificationRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationEventHandler> _logger;

    public NotificationEventHandler(
        INotificationRepository repository,
        IEmailSender emailSender,
        ILogger<NotificationEventHandler> logger)
    {
        _repository = repository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task HandlePaymentSucceededAsync(PaymentSucceededEvent evt, CancellationToken cancellationToken = default)
    {
        if (await _repository.HasProcessedAsync(evt.EventId, cancellationToken))
        {
            _logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        var logs = new List<NotificationLog>();

        // Notify the payee - always happens, every payment type has a payee
        logs.Add(await BuildAndSendAsync(
            evt, recipientAccountId: evt.PayeeId,
            subject: "Payment received",
            body: $"You received {evt.Amount} {evt.Currency} (Payment ID: {evt.PaymentId}).",
            cancellationToken));

        // Notify the payer - only exists for Transfer/MerchantPayment, not TopUp
        if (evt.PayerId is Guid payerId)
        {
            logs.Add(await BuildAndSendAsync(
                evt, recipientAccountId: payerId,
                subject: "Payment sent",
                body: $"You sent {evt.Amount} {evt.Currency} (Payment ID: {evt.PaymentId}).",
                cancellationToken));
        }

        await _repository.AddLogsAndMarkProcessedAsync(logs, evt.EventId, nameof(PaymentSucceededEvent), cancellationToken);
    }

    private async Task<NotificationLog> BuildAndSendAsync(
        PaymentSucceededEvent evt,
        Guid recipientAccountId,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            PaymentId = evt.PaymentId,
            RecipientAccountId = recipientAccountId,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var toEmail = "trymesan204@gmail.com";

            await _emailSender.SendAsync(toEmail, subject, body, cancellationToken);

            log.Status = NotificationStatus.Sent;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification email for payment {PaymentId} to account {RecipientAccountId}",
                evt.PaymentId, recipientAccountId);

            log.Status = NotificationStatus.Failed;
            log.FailureReason = ex.Message;
        }

        return log;
    }
}
