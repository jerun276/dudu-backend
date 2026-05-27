using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/coins")]
public class CoinsController(ICoinsService coinsService) : ControllerBase
{
    [HttpGet("balance")]
    public async Task<ActionResult<CoinBalanceResponse>> GetBalance([FromQuery] Guid childId, CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await coinsService.GetBalanceAsync(userId, childId, ct);
        return Ok(response);
    }

    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
        {
            throw new UnauthorizedException("Missing authentication");
        }
        return userId;
    }
}
