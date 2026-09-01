using LedgerService.Api.Abstractions;
using LedgerService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LedgerService.Api.Controllers;

[ApiController]
public class LedgerServiceController : ControllerBase
{
    private readonly ILedgerQueryService _ledgerQueryService;

    public LedgerServiceController(ILedgerQueryService ledgerQueryService)
    {
        _ledgerQueryService = ledgerQueryService;
    }

    [HttpGet("accounts/{accountId:guid}/balance")]
    [ProducesResponseType(typeof(AccountBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountBalanceResponse>> GetAccountBalance(Guid accountId, CancellationToken cancellationToken)
    {
        var balance = await _ledgerQueryService.GetAccountBalanceAsync(accountId, cancellationToken);

        if (balance is null)
        {
            return NotFound();
        }

        return Ok(balance);
    }

    [HttpGet("accounts/{accountId:guid}/entries")]
    [ProducesResponseType(typeof(AccountEntriesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountEntriesResponse>> GetAccountEntries(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var result = await _ledgerQueryService.GetAccountEntriesAsync(accountId, page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("payments/{paymentId:guid}/entries")]
    [ProducesResponseType(typeof(List<PaymentLedgerEntryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentLedgerEntryResponse>>> GetPaymentEntries(Guid paymentId, CancellationToken cancellationToken)
    {
        var entries = await _ledgerQueryService.GetPaymentEntriesAsync(paymentId, cancellationToken);

        return Ok(entries);
    }
}
