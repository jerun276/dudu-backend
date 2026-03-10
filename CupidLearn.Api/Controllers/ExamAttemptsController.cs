using System.Security.Claims;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/progress")]
public class ExamAttemptsController(IExamAttemptService examAttemptService) : ControllerBase
{
    [HttpGet("exam-attempts/{childId:guid}/{examId:guid}")]
    public async Task<ActionResult<ExamAttemptResponse>> Get(Guid childId, Guid examId, CancellationToken ct)
    {
        var userId = GetUserId();
        var attempt = await examAttemptService.GetAsync(userId, childId, examId, ct);
        if (attempt == null)
        {
            return NotFound();
        }
        return Ok(attempt);
    }

    [HttpGet("exam-attempts/{childId:guid}/{examId:guid}/can-attempt")]
    public async Task<ActionResult<bool>> CanAttempt(Guid childId, Guid examId, CancellationToken ct)
    {
        var userId = GetUserId();
        var can = await examAttemptService.CanAttemptAsync(userId, childId, examId, ct);
        return Ok(can);
    }

    [HttpGet("exam-attempts/{childId:guid}/{examId:guid}/has-passed")]
    public async Task<ActionResult<bool>> HasPassed(Guid childId, Guid examId, [FromQuery] int passingScore, CancellationToken ct)
    {
        var userId = GetUserId();
        var hasPassed = await examAttemptService.HasPassedAsync(userId, childId, examId, passingScore, ct);
        return Ok(hasPassed);
    }

    [HttpPost("exam-attempts")]
    public async Task<ActionResult<ExamAttemptResponse>> Record(
        [FromQuery] Guid childId,
        [FromQuery] Guid examId,
        [FromQuery] int score,
        [FromQuery] int passingScore,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await examAttemptService.RecordAttemptAsync(userId, childId, examId, score, passingScore, ct);
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
