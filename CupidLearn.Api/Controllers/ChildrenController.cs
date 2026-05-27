using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Profiles;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles/children")]
public class ChildrenController(IChildrenService childrenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChildProfileResponse>> Create([FromBody] ChildProfileCreateRequest request, CancellationToken ct)
    {
        var parentUserId = GetUserId();

        var response = await childrenService.CreateAsync(parentUserId, request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<ActionResult<List<ChildProfileResponse>>> List(CancellationToken ct)
    {
        var parentUserId = GetUserId();
        var children = await childrenService.ListAsync(parentUserId, ct);
        return Ok(children);
    }

    [HttpPut("{childId:guid}")]
    public async Task<ActionResult<ChildProfileResponse>> Update(Guid childId, [FromBody] ChildProfileUpdateRequest request, CancellationToken ct)
    {
        var parentUserId = GetUserId();
        var response = await childrenService.UpdateAsync(parentUserId, childId, request, ct);
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
