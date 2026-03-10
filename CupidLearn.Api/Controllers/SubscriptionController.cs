using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Billing;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
public class SubscriptionController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpGet("subscriptions/{userId:guid}")]
    public async Task<ActionResult<SubscriptionResponse>> Get(Guid userId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var response = await subscriptionService.GetByUserIdAsync(authUserId, role, userId, ct);
        return Ok(response);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("subscriptions/{userId:guid}")]
    public async Task<ActionResult<SubscriptionResponse>> Upsert(Guid userId, [FromBody] SubscriptionUpsertRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var response = await subscriptionService.UpsertAsync(authUserId, role, userId, request, ct);
        return Ok(response);
    }

    [HttpGet("subscriptions/{userId:guid}/limits")]
    public async Task<ActionResult<SubscriptionLimitsResponse>> Limits(Guid userId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var response = await subscriptionService.GetLimitsAsync(authUserId, role, userId, ct);
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
