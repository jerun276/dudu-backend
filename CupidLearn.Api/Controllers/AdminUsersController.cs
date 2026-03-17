using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Admin;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/users")]
public class AdminUsersController(IAdminUsersService adminUsersService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminUserSearchResponse>> Search([FromQuery] string? query, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var resp = await adminUsersService.SearchAsync(authUserId, role, query, skip, take, ct);
        return Ok(resp);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<AdminUserSummaryResponse>> Get(Guid userId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var resp = await adminUsersService.GetSummaryAsync(authUserId, role, userId, ct);
        return Ok(resp);
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
