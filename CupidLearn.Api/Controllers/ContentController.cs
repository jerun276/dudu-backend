using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Route("api/content")]
public class ContentController(IContentService contentService) : ControllerBase
{
    [HttpGet("levels")]
    public async Task<ActionResult<List<LevelResponse>>> ListLevels(CancellationToken ct)
    {
        var levels = await contentService.ListLevelsAsync(ct);
        return Ok(levels);
    }

    [HttpGet("levels/{levelId:guid}/modules")]
    public async Task<ActionResult<List<ModuleResponse>>> ListModules(Guid levelId, CancellationToken ct)
    {
        var modules = await contentService.ListModulesByLevelAsync(levelId, ct);
        return Ok(modules);
    }

    [HttpGet("modules/{moduleId:guid}/exams")]
    public async Task<ActionResult<List<ExamResponse>>> ListExams(Guid moduleId, CancellationToken ct)
    {
        var exams = await contentService.ListExamsByModuleAsync(moduleId, ct);
        return Ok(exams);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("levels")]
    public async Task<ActionResult<LevelResponse>> CreateLevel([FromBody] LevelCreateRequest request, CancellationToken ct)
    {
        var level = await contentService.CreateLevelAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, level);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("levels/{levelId:guid}/modules")]
    public async Task<ActionResult<ModuleResponse>> CreateModule(Guid levelId, [FromBody] ModuleCreateRequest request, CancellationToken ct)
    {
        var module = await contentService.CreateModuleAsync(levelId, request, ct);
        return StatusCode(StatusCodes.Status201Created, module);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("modules/{moduleId:guid}/exams")]
    public async Task<ActionResult<ExamResponse>> CreateExam(Guid moduleId, [FromBody] ExamCreateRequest request, CancellationToken ct)
    {
        var exam = await contentService.CreateExamAsync(moduleId, request, ct);
        return StatusCode(StatusCodes.Status201Created, exam);
    }
}
