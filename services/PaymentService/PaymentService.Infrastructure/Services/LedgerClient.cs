using System.Net.Http.Json;
using System.Text.Json;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

public class LedgerClient : ILedgerClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public LedgerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<AccountBalanceResponse>(
            $"accounts/{accountId}/balance", JsonOptions, cancellationToken);

        return response?.Balance ?? 0m;
    }

    private class AccountBalanceResponse
    {
        public Guid AccountId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime AsOf { get; set; }
    }
}
