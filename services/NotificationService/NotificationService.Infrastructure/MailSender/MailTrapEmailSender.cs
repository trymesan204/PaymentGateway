using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NotificationService.Domain.Abstractions;

namespace NotificationService.Infrastructure.MailSender;

public class MailtrapEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailtrapEmailSender> _logger;

    public MailtrapEmailSender(IConfiguration configuration, ILogger<MailtrapEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("PaymentGateway", "noreply@paymentgateway.dev"));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Mailtrap:Host"],
            int.Parse(_configuration["Mailtrap:Port"]!),
            SecureSocketOptions.StartTls,
            cancellationToken);
        await client.AuthenticateAsync(
            _configuration["Mailtrap:Username"],
            _configuration["Mailtrap:Password"],
            cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Recipient}: {Subject}", toEmail, subject);
    }
}
