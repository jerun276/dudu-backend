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

    [HttpPost("{userId:guid}/children")]
    public async Task<ActionResult<AdminChildResponse>> CreateChild(Guid userId, [FromBody] AdminChildCreateRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var resp = await adminUsersService.CreateChildAsync(authUserId, role, userId, request, ct);
        return StatusCode(StatusCodes.Status201Created, resp);
    }

    [HttpPut("{userId:guid}/children/{childId:guid}")]
    public async Task<ActionResult<AdminChildResponse>> UpdateChild(Guid userId, Guid childId, [FromBody] AdminChildUpdateRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        var resp = await adminUsersService.UpdateChildAsync(authUserId, role, childId, request, ct);
        return Ok(resp);
    }

    [HttpDelete("{userId:guid}/children/{childId:guid}")]
    public async Task<IActionResult> DeleteChild(Guid userId, Guid childId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        await adminUsersService.DeleteChildAsync(authUserId, role, childId, ct);
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
