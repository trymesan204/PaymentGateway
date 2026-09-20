using LedgerService.Api.Abstractions;
using LedgerService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LedgerService.Api.Controllers;

[ApiController]
[Route("dev")]
public class DevController : ControllerBase
{
    private readonly IDevSeedService _devSeedService;
    private readonly IWebHostEnvironment _environment;

    public DevController(IDevSeedService devSeedService, IWebHostEnvironment environment)
    {
        _devSeedService = devSeedService;
        _environment = environment;
    }

    // Dev-only shortcut: writes a balanced debit/credit pair directly, skipping the RabbitMQ
    // event flow, so a test account can be funded without a real TopUp payment. 404s outside
    // Development so it can never be reachable in a real deployment.
    [HttpPost("accounts/{accountId:guid}/seed-balance")]
    [ProducesResponseType(typeof(SeedBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeedBalanceResponse>> SeedBalance(
        Guid accountId, [FromBody] SeedBalanceRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _devSeedService.SeedBalanceAsync(accountId, request, cancellationToken);
        return Ok(result);
    }
}
