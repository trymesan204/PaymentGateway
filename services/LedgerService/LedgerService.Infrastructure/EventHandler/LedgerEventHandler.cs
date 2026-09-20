using LedgerService.Domain.Abstractions;
using LedgerService.Domain.Constants;
using LedgerService.Domain.Entities;
using LedgerService.Domain.Enums;
using Microsoft.Extensions.Logging;
using PaymentGateway.Contracts;

namespace LedgerService.Infrastructure.EventHandler;

public class LedgerEventHandler : ILedgerEventHandler
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ILogger<LedgerEventHandler> _logger;

    public LedgerEventHandler(ILedgerRepository ledgerRepository, ILogger<LedgerEventHandler> logger)
    {
        _ledgerRepository = ledgerRepository;
        _logger = logger;
    }

    public async Task HandlePaymentSucceededAsync(PaymentSucceededEvent evt, CancellationToken cancellationToken = default)
    {
        if (await _ledgerRepository.HasProcessedAsync(evt.EventId, cancellationToken))
        {
            _logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
            return;
        }

        var debitAccountId = evt.PayerId ?? LedgerAccounts.SystemSuspenseAccountId;
        var creditAccountId = evt.PayeeId;

        var debitBalance = await _ledgerRepository.GetLatestBalanceAsync(debitAccountId, cancellationToken);
        var creditBalance = await _ledgerRepository.GetLatestBalanceAsync(creditAccountId, cancellationToken);

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            PaymentId = evt.PaymentId,
            AccountId = debitAccountId,
            Type = LedgerEntryType.Debit,
            Amount = evt.Amount,
            Currency = evt.Currency,
            BalanceAfter = debitBalance - evt.Amount,
            CreatedAt = DateTime.UtcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            PaymentId = evt.PaymentId,
            AccountId = creditAccountId,
            Type = LedgerEntryType.Credit,
            Amount = evt.Amount,
            Currency = evt.Currency,
            BalanceAfter = creditBalance + evt.Amount,
            CreatedAt = DateTime.UtcNow
        };

        await _ledgerRepository.AddEntriesAndMarkProcessedAsync(
            debitEntry, creditEntry, evt.EventId, nameof(PaymentSucceededEvent), cancellationToken);

        _logger.LogInformation(
            "Recorded entries for payment {PaymentId}: debit {DebitAccount} -{Amount}, credit {CreditAccount} +{Amount}",
            evt.PaymentId, debitAccountId, evt.Amount, creditAccountId, evt.Amount);
    }
}