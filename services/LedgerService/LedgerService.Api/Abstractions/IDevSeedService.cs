using LedgerService.Api.Models;

namespace LedgerService.Api.Abstractions;

public interface IDevSeedService
{
    Task<SeedBalanceResponse> SeedBalanceAsync(
        Guid accountId, SeedBalanceRequest request, CancellationToken cancellationToken = default);
}
