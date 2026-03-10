using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Billing;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
public class OrganizationController(IOrganizationSeatService organizationSeatService) : ControllerBase
{
    [HttpPost("organizations")]
    public async Task<ActionResult<CreateOrganizationResponse>> Create([FromBody] CreateOrganizationRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var resp = await organizationSeatService.CreateOrganizationAsync(authUserId, role, request, ct);
        return StatusCode(StatusCodes.Status201Created, resp);
    }

    [HttpGet("organizations/{organizationId:guid}/seats")]
    public async Task<ActionResult<List<SeatResponse>>> ListSeats(Guid organizationId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var seats = await organizationSeatService.ListSeatsAsync(authUserId, role, organizationId, ct);
        return Ok(seats);
    }

    [HttpPost("organizations/{organizationId:guid}/seats/assign")]
    public async Task<ActionResult<SeatResponse>> Assign(Guid organizationId, [FromBody] AssignSeatRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var seat = await organizationSeatService.AssignSeatAsync(authUserId, role, organizationId, request.UserId, ct);
        return Ok(seat);
    }

    [HttpPost("organizations/{organizationId:guid}/seats/revoke")]
    public async Task<ActionResult<SeatResponse>> Revoke(Guid organizationId, [FromBody] RevokeSeatRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var seat = await organizationSeatService.RevokeSeatAsync(authUserId, role, organizationId, request.UserId, ct);
        return Ok(seat);
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
