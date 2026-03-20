using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Route("api/content")]
public class ExpandedContentController(IContentQueryService query, IContentAdminService admin, IContentService content) : ControllerBase
{
    [HttpGet("activity-types")]
    public async Task<ActionResult<List<ActivityTypeResponse>>> ListActivityTypes(CancellationToken ct)
    {
        var types = await query.ListActivityTypesAsync(ct);
        return Ok(types);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("activity-types")]
    public async Task<ActionResult<ActivityTypeResponse>> CreateActivityType([FromBody] ActivityTypeCreateRequest request, CancellationToken ct)
    {
        var created = await admin.CreateActivityTypeAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("activity-types/{activityTypeId:guid}")]
    public async Task<ActionResult<ActivityTypeResponse>> UpdateActivityType(Guid activityTypeId, [FromBody] ActivityTypeUpdateRequest request, CancellationToken ct)
    {
        var updated = await admin.UpdateActivityTypeAsync(activityTypeId, request, ct);
        return Ok(updated);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("activity-types/{activityTypeId:guid}")]
    public async Task<IActionResult> DeleteActivityType(Guid activityTypeId, CancellationToken ct)
    {
        await admin.DeleteActivityTypeAsync(activityTypeId, ct);
        return NoContent();
    }

    [HttpGet("levels")]
    public async Task<ActionResult<List<LevelResponse>>> ListLevels([FromQuery] string? language, CancellationToken ct)
    {
        var levels = await query.ListLevelsAsync(language, ct);
        return Ok(levels);
    }

    [HttpGet("levels/{levelId:guid}")]
    public async Task<ActionResult<LevelResponse>> GetLevel(Guid levelId, CancellationToken ct)
    {
        var level = await query.GetLevelByIdAsync(levelId, ct);
        return Ok(level);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("levels")]
    public async Task<ActionResult<LevelResponse>> CreateLevel([FromBody] LevelCreateRequest request, CancellationToken ct)
    {
        var level = await content.CreateLevelAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, level);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("levels/{levelId:guid}")]
    public async Task<ActionResult<LevelResponse>> UpdateLevel(Guid levelId, [FromBody] LevelUpdateRequest request, CancellationToken ct)
    {
        var level = await content.UpdateLevelAsync(levelId, request, ct);
        return Ok(level);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("levels/{levelId:guid}")]
    public async Task<IActionResult> DeleteLevel(Guid levelId, CancellationToken ct)
    {
        await content.DeleteLevelAsync(levelId, ct);
        return NoContent();
    }

    [HttpGet("levels/{levelId:guid}/modules")]
    public async Task<ActionResult<List<ModuleResponse>>> ListModules(Guid levelId, CancellationToken ct)
    {
        var modules = await query.ListModulesByLevelAsync(levelId, ct);
        return Ok(modules);
    }

    [HttpGet("modules/{moduleId:guid}")]
    public async Task<ActionResult<ModuleResponse>> GetModule(Guid moduleId, CancellationToken ct)
    {
        var module = await query.GetModuleByIdAsync(moduleId, ct);
        return Ok(module);
    }

    [HttpGet("modules/{moduleId:guid}/lessons")]
    public async Task<ActionResult<List<LessonResponse>>> ListLessons(Guid moduleId, CancellationToken ct)
    {
        var lessons = await query.ListLessonsByModuleAsync(moduleId, ct);
        return Ok(lessons);
    }

    [HttpGet("lessons/{lessonId:guid}")]
    public async Task<ActionResult<LessonResponse>> GetLesson(Guid lessonId, CancellationToken ct)
    {
        var lesson = await query.GetLessonByIdAsync(lessonId, ct);
        return Ok(lesson);
    }

    [HttpGet("lessons/{lessonId:guid}/activities")]
    public async Task<ActionResult<List<ActivityResponse>>> ListActivities(Guid lessonId, CancellationToken ct)
    {
        var activities = await query.ListActivitiesByLessonAsync(lessonId, ct);
        return Ok(activities);
    }

    [HttpGet("lessons/{lessonId:guid}/quizzes")]
    public async Task<ActionResult<List<QuizResponse>>> ListQuizzes(Guid lessonId, CancellationToken ct)
    {
        var quizzes = await query.ListQuizzesByLessonAsync(lessonId, ct);
        return Ok(quizzes);
    }

    [HttpGet("quizzes/{quizId:guid}")]
    public async Task<ActionResult<QuizResponse>> GetQuiz(Guid quizId, CancellationToken ct)
    {
        var quiz = await query.GetQuizByIdAsync(quizId, ct);
        return Ok(quiz);
    }

    [HttpGet("quizzes/{quizId:guid}/questions")]
    public async Task<ActionResult<List<QuizQuestionResponse>>> ListQuestions(Guid quizId, CancellationToken ct)
    {
        var questions = await query.ListQuestionsByQuizAsync(quizId, ct);
        return Ok(questions);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("modules")]
    public async Task<ActionResult<ModuleResponse>> CreateModule([FromBody] ModuleCreateV2Request request, CancellationToken ct)
    {
        var module = await admin.CreateModuleAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, module);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("levels/{levelId:guid}/modules")]
    public async Task<ActionResult<ModuleResponse>> CreateModuleUnderLevel(Guid levelId, [FromBody] ModuleCreateRequest request, CancellationToken ct)
    {
        var module = await content.CreateModuleAsync(levelId, request, ct);
        return StatusCode(StatusCodes.Status201Created, module);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("modules/{moduleId:guid}")]
    public async Task<ActionResult<ModuleResponse>> UpdateModule(Guid moduleId, [FromBody] ModuleUpdateRequest request, CancellationToken ct)
    {
        var module = await admin.UpdateModuleAsync(moduleId, request, ct);
        return Ok(module);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("modules/{moduleId:guid}")]
    public async Task<IActionResult> DeleteModule(Guid moduleId, CancellationToken ct)
    {
        await admin.DeleteModuleAsync(moduleId, ct);
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("modules/{moduleId:guid}/lessons")]
    public async Task<ActionResult<LessonResponse>> CreateLesson(Guid moduleId, [FromBody] LessonCreateRequest request, CancellationToken ct)
    {
        var lesson = await admin.CreateLessonAsync(moduleId, request, ct);
        return StatusCode(StatusCodes.Status201Created, lesson);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("lessons/{lessonId:guid}")]
    public async Task<ActionResult<LessonResponse>> UpdateLesson(Guid lessonId, [FromBody] LessonUpdateRequest request, CancellationToken ct)
    {
        var lesson = await admin.UpdateLessonAsync(lessonId, request, ct);
        return Ok(lesson);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("lessons/{lessonId:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid lessonId, CancellationToken ct)
    {
        await admin.DeleteLessonAsync(lessonId, ct);
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("lessons/{lessonId:guid}/activities")]
    public async Task<ActionResult<ActivityResponse>> CreateActivity(Guid lessonId, [FromBody] ActivityCreateRequest request, CancellationToken ct)
    {
        var activity = await admin.CreateActivityAsync(lessonId, request, ct);
        return StatusCode(StatusCodes.Status201Created, activity);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("activities/{activityId:guid}")]
    public async Task<ActionResult<ActivityResponse>> GetActivity(Guid activityId, CancellationToken ct)
    {
        var activity = await query.GetActivityByIdAsync(activityId, ct);
        return Ok(activity);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("activities/{activityId:guid}")]
    public async Task<ActionResult<ActivityResponse>> UpdateActivity(Guid activityId, [FromBody] ActivityUpdateRequest request, CancellationToken ct)
    {
        var activity = await admin.UpdateActivityAsync(activityId, request, ct);
        return Ok(activity);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("activities/{activityId:guid}")]
    public async Task<IActionResult> DeleteActivity(Guid activityId, CancellationToken ct)
    {
        await admin.DeleteActivityAsync(activityId, ct);
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("lessons/{lessonId:guid}/quizzes")]
    public async Task<ActionResult<QuizResponse>> CreateQuiz(Guid lessonId, [FromBody] QuizCreateRequest request, CancellationToken ct)
    {
        var quiz = await admin.CreateQuizAsync(lessonId, request, ct);
        return StatusCode(StatusCodes.Status201Created, quiz);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("quizzes/{quizId:guid}")]
    public async Task<ActionResult<QuizResponse>> UpdateQuiz(Guid quizId, [FromBody] QuizUpdateRequest request, CancellationToken ct)
    {
        var quiz = await admin.UpdateQuizAsync(quizId, request, ct);
        return Ok(quiz);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("quizzes/{quizId:guid}")]
    public async Task<IActionResult> DeleteQuiz(Guid quizId, CancellationToken ct)
    {
        await admin.DeleteQuizAsync(quizId, ct);
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("quizzes/{quizId:guid}/questions")]
    public async Task<ActionResult<QuizQuestionResponse>> CreateQuestion(Guid quizId, [FromBody] QuizQuestionCreateRequest request, CancellationToken ct)
    {
        var q = await admin.CreateQuizQuestionAsync(quizId, request, ct);
        return StatusCode(StatusCodes.Status201Created, q);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("questions/{questionId:guid}")]
    public async Task<ActionResult<QuizQuestionResponse>> UpdateQuestion(Guid questionId, [FromBody] QuizQuestionUpdateRequest request, CancellationToken ct)
    {
        var q = await admin.UpdateQuizQuestionAsync(questionId, request, ct);
        return Ok(q);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId, CancellationToken ct)
    {
        await admin.DeleteQuizQuestionAsync(questionId, ct);
        return NoContent();
    }
}
