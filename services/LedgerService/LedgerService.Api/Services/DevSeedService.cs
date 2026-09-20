using LedgerService.Api.Abstractions;
using LedgerService.Api.Models;
using LedgerService.Domain.Abstractions;
using LedgerService.Domain.Constants;
using LedgerService.Domain.Entities;
using LedgerService.Domain.Enums;

namespace LedgerService.Api.Services;

public class DevSeedService : IDevSeedService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ILogger<DevSeedService> _logger;

    public DevSeedService(ILedgerRepository ledgerRepository, ILogger<DevSeedService> logger)
    {
        _ledgerRepository = ledgerRepository;
        _logger = logger;
    }

    public async Task<SeedBalanceResponse> SeedBalanceAsync(
        Guid accountId, SeedBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var suspenseBalance = await _ledgerRepository.GetLatestBalanceAsync(
            LedgerAccounts.SystemSuspenseAccountId, cancellationToken);
        var accountBalance = await _ledgerRepository.GetLatestBalanceAsync(accountId, cancellationToken);

        var paymentId = Guid.NewGuid();

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            AccountId = LedgerAccounts.SystemSuspenseAccountId,
            Type = LedgerEntryType.Debit,
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceAfter = suspenseBalance - request.Amount,
            CreatedAt = DateTime.UtcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            AccountId = accountId,
            Type = LedgerEntryType.Credit,
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceAfter = accountBalance + request.Amount,
            CreatedAt = DateTime.UtcNow
        };

        await _ledgerRepository.AddEntriesAndMarkProcessedAsync(
            debitEntry, creditEntry, Guid.NewGuid(), "DevSeedBalance", cancellationToken);

        _logger.LogInformation(
            "Dev-seeded balance for account {AccountId}: +{Amount} {Currency}, new balance {Balance}",
            accountId, request.Amount, request.Currency, creditEntry.BalanceAfter);

        return new SeedBalanceResponse { AccountId = accountId, Balance = creditEntry.BalanceAfter };
    }
}
