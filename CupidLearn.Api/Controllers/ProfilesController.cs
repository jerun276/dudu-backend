using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Profiles;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController(IProfilesService profilesService) : ControllerBase
{
    [Authorize]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ProfileResponse>> Get(Guid userId, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var authRole = User.FindFirstValue(ClaimTypes.Role);

        var profile = await profilesService.GetByUserIdAsync(authUserId, authRole, userId, ct);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<ProfileResponse>> Upsert(Guid userId, [FromBody] ProfileUpsertRequest request, CancellationToken ct)
    {
        var authUserId = GetUserId();
        var response = await profilesService.UpsertAsync(authUserId, userId, request, ct);
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
