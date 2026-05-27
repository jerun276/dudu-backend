using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/badges")]
public class BadgesController(IBadgeService badgeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BadgeResponse>>> ListBadges([FromQuery] Guid childId, CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await badgeService.ListBadgesAsync(userId, childId, ct);
        return Ok(response);
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromQuery] Guid childId, CancellationToken ct)
    {
        var userId = GetUserId();
        await badgeService.EvaluateAndAwardAsync(userId, childId, ct);
        return NoContent();
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
