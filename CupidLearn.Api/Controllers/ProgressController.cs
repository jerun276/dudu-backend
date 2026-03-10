using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/progress")]
public class ProgressController(IProgressService progressService) : ControllerBase
{
    [HttpPost("attempts")]
    public async Task<ActionResult<AttemptResponse>> RecordAttempt([FromBody] AttemptCreateRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await progressService.RecordAttemptAsync(userId, request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("lessons/{lessonId:guid}/complete")]
    public async Task<ActionResult<LessonProgressResponse>> CompleteLesson(Guid lessonId, [FromBody] LessonCompleteRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await progressService.CompleteLessonAsync(userId, lessonId, request, ct);
        return Ok(response);
    }

    [HttpGet("lessons")]
    public async Task<ActionResult<List<LessonProgressResponse>>> ListLessonProgress(CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await progressService.ListLessonProgressAsync(userId, ct);
        return Ok(response);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ProgressSummaryResponse>> Summary(CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await progressService.SummaryAsync(userId, ct);
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
