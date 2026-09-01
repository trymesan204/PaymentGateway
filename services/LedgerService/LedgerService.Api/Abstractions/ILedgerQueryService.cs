using LedgerService.Api.Models;

namespace LedgerService.Api.Abstractions;

public interface ILedgerQueryService
{
    Task<AccountBalanceResponse?> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<AccountEntriesResponse> GetAccountEntriesAsync(
        Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<List<PaymentLedgerEntryResponse>> GetPaymentEntriesAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
