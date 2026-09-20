namespace PaymentService.Domain.Interfaces;

public interface ILedgerClient
{
    Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
}
