using System.Net.Http.Json;
using System.Text.Json;
using CupidLearn.Web.Models.Content;
using CupidLearn.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Web.Controllers;

[Authorize(Roles = "ADMIN")]
public class ContentController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? language, [FromQuery] Guid? levelId, [FromQuery] Guid? moduleId, [FromQuery] Guid? lessonId, [FromQuery] Guid? quizId, CancellationToken ct)
    {
        var vm = new ContentIndexViewModel
        {
            Language = language,
            SelectedLevelId = levelId,
            SelectedModuleId = moduleId,
            SelectedLessonId = lessonId,
            SelectedQuizId = quizId
        };

        try
        {
            var client = apiClient.CreateAuthenticatedClient();

            var levels = await client.GetFromJsonAsync<List<LevelDto>>($"/api/content/levels?language={Uri.EscapeDataString(language ?? string.Empty)}", ct) ?? [];
            vm.Levels = levels.Select(x => new ContentIndexViewModel.LevelVm(x.Id, x.Language, x.Name)).ToList();

            if (levelId.HasValue)
            {
                var modules = await client.GetFromJsonAsync<List<ModuleDto>>($"/api/content/levels/{levelId.Value}/modules", ct) ?? [];
                vm.Modules = modules.Select(x => new ContentIndexViewModel.ModuleVm(x.Id, x.LevelId, x.Name)).ToList();
            }

            if (moduleId.HasValue)
            {
                var lessons = await client.GetFromJsonAsync<List<LessonDto>>($"/api/content/modules/{moduleId.Value}/lessons", ct) ?? [];
                vm.Lessons = lessons.Select(x => new ContentIndexViewModel.LessonVm(x.Id, x.ModuleId, x.Title, x.Description, x.OrderIndex)).ToList();
            }

            if (lessonId.HasValue)
            {
                var activities = await client.GetFromJsonAsync<List<ActivityDto>>($"/api/content/lessons/{lessonId.Value}/activities", ct) ?? [];
                vm.Activities = activities.Select(x => new ContentIndexViewModel.ActivityVm(x.Id, x.LessonId, x.Type, x.Title, x.ImageUrl, x.OrderIndex)).ToList();

                var quizzes = await client.GetFromJsonAsync<List<QuizDto>>($"/api/content/lessons/{lessonId.Value}/quizzes", ct) ?? [];
                vm.Quizzes = quizzes.Select(x => new ContentIndexViewModel.QuizVm(x.Id, x.LessonId, x.Title)).ToList();
            }

            if (quizId.HasValue)
            {
                var questions = await client.GetFromJsonAsync<List<QuizQuestionDto>>($"/api/content/quizzes/{quizId.Value}/questions", ct) ?? [];
                vm.QuizQuestions = questions.Select(x => new ContentIndexViewModel.QuizQuestionVm(x.Id, x.QuizId, x.Prompt, x.CorrectOption)).ToList();
            }
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModule(CreateModuleViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { levelId = model.LevelId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync("/api/content/modules", new { levelId = model.LevelId, name = model.Name }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", new { levelId = model.LevelId });
        }

        return RedirectToAction("Index", new { levelId = model.LevelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(CreateLessonViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { moduleId = model.ModuleId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync($"/api/content/modules/{model.ModuleId}/lessons", new
        {
            title = model.Title,
            description = model.Description,
            content = model.Content,
            orderIndex = model.OrderIndex
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", new { moduleId = model.ModuleId });
        }

        return RedirectToAction("Index", new { moduleId = model.ModuleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActivity(CreateActivityViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { lessonId = model.LessonId });
        }

        JsonElement? payload = null;
        if (!string.IsNullOrWhiteSpace(model.PayloadJson))
        {
            try
            {
                payload = JsonSerializer.Deserialize<JsonElement>(model.PayloadJson);
            }
            catch
            {
                return RedirectToAction("Index", new { lessonId = model.LessonId });
            }
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync($"/api/content/lessons/{model.LessonId}/activities", new
        {
            type = model.Type,
            title = model.Title,
            imageUrl = model.ImageUrl,
            payload,
            orderIndex = model.OrderIndex
        }, ct);

        return RedirectToAction("Index", new { lessonId = model.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuiz(CreateQuizViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { lessonId = model.LessonId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync($"/api/content/lessons/{model.LessonId}/quizzes", new { title = model.Title }, ct);
        return RedirectToAction("Index", new { lessonId = model.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuizQuestion(CreateQuizQuestionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { quizId = model.QuizId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync($"/api/content/quizzes/{model.QuizId}/questions", new
        {
            prompt = model.Prompt,
            optionA = model.OptionA,
            optionB = model.OptionB,
            optionC = model.OptionC,
            optionD = model.OptionD,
            correctOption = model.CorrectOption
        }, ct);

        return RedirectToAction("Index", new { quizId = model.QuizId });
    }

    private sealed record LevelDto(Guid Id, string Language, string Name);
    private sealed record ModuleDto(Guid Id, Guid LevelId, string Name);
    private sealed record LessonDto(Guid Id, Guid ModuleId, string Title, string? Description, string? Content, int OrderIndex);
    private sealed record ActivityDto(Guid Id, Guid LessonId, string Type, string Title, string? ImageUrl, JsonElement? Payload, int OrderIndex);
    private sealed record QuizDto(Guid Id, Guid LessonId, string Title);
    private sealed record QuizQuestionDto(Guid Id, Guid QuizId, string Prompt, string OptionA, string OptionB, string OptionC, string OptionD, string CorrectOption, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
