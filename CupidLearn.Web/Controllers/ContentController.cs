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
    private IActionResult RedirectToIndexWithContext(string? language, Guid? levelId, Guid? moduleId, Guid? lessonId, Guid? quizId)
    {
        return RedirectToAction("Index", new { language, levelId, moduleId, lessonId, quizId });
    }

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

            var activityTypes = await client.GetFromJsonAsync<List<ActivityTypeDto>>("/api/content/activity-types", ct) ?? [];
            vm.ActivityTypes = activityTypes
                .Select(x => new ContentIndexViewModel.ActivityTypeVm(x.Id, x.Key, x.DisplayName, x.Description, x.Schema == null ? null : JsonSerializer.Serialize(x.Schema)))
                .ToList();

            var levels = await client.GetFromJsonAsync<List<LevelDto>>($"/api/content/levels?language={Uri.EscapeDataString(language ?? string.Empty)}", ct) ?? [];
            vm.Levels = levels.Select(x => new ContentIndexViewModel.LevelVm(x.Id, x.Language, x.Name)).ToList();

            if (levelId.HasValue)
            {
                var modules = await client.GetFromJsonAsync<List<ModuleDto>>($"/api/content/levels/{levelId.Value}/modules", ct) ?? [];
                vm.Modules = modules.Select(x => new ContentIndexViewModel.ModuleVm(x.Id, x.LevelId, x.Name, x.OrderIndex)).ToList();
            }

            if (moduleId.HasValue)
            {
                var lessons = await client.GetFromJsonAsync<List<LessonDto>>($"/api/content/modules/{moduleId.Value}/lessons", ct) ?? [];
                vm.Lessons = lessons.Select(x => new ContentIndexViewModel.LessonVm(x.Id, x.ModuleId, x.Title, x.Description, x.OrderIndex)).ToList();
            }

            if (lessonId.HasValue)
            {
                var activities = await client.GetFromJsonAsync<List<ActivityDto>>($"/api/content/lessons/{lessonId.Value}/activities", ct) ?? [];
                vm.Activities = activities.Select(x => new ContentIndexViewModel.ActivityVm(x.Id, x.LessonId, x.Type, x.Title, x.ImageUrl, x.OrderIndex, x.Payload.HasValue ? JsonSerializer.Serialize(x.Payload.Value) : null)).ToList();

                var quizzes = await client.GetFromJsonAsync<List<QuizDto>>($"/api/content/lessons/{lessonId.Value}/quizzes", ct) ?? [];
                vm.Quizzes = quizzes.Select(x => new ContentIndexViewModel.QuizVm(x.Id, x.LessonId, x.Title)).ToList();
            }

            if (quizId.HasValue)
            {
                var questions = await client.GetFromJsonAsync<List<QuizQuestionDto>>($"/api/content/quizzes/{quizId.Value}/questions", ct) ?? [];
                vm.QuizQuestions = questions
                    .Select(x => new ContentIndexViewModel.QuizQuestionVm(x.Id, x.QuizId, x.Prompt, x.OptionA, x.OptionB, x.OptionC, x.OptionD, x.CorrectOption))
                    .ToList();
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
    public async Task<IActionResult> CreateLevel(CreateLevelViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a level name.";
            return RedirectToAction("Index", new { language = model.Language });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync("/api/content/levels", new
        {
            language = model.Language,
            name = model.Name
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to create level. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { language = model.Language });
        }

        TempData["Success"] = "Level created.";
        return RedirectToAction("Index", new { language = model.Language });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLevel(UpdateLevelViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a level name.";
            return RedirectToAction("Index", new { language = model.Language });
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/levels/{model.Id}", new
        {
            language = model.Language,
            name = model.Name
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update level. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { language = model.Language, levelId = model.Id });
        }

        TempData["Success"] = "Level updated.";
        return RedirectToAction("Index", new { language = model.Language, levelId = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLevel([FromForm] Guid id, [FromForm] string? language, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/levels/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete level. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { language, levelId = id });
        }

        TempData["Success"] = "Level deleted.";
        return RedirectToAction("Index", new { language });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActivityType(CreateActivityTypeViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        JsonElement? schema = null;
        if (!string.IsNullOrWhiteSpace(model.SchemaJson))
        {
            try
            {
                schema = JsonSerializer.Deserialize<JsonElement>(model.SchemaJson);
            }
            catch
            {
                TempData["Error"] = "Schema JSON is not valid JSON. Please fix it and try again.";
                return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
            }
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync("/api/content/activity-types", new
        {
            key = model.Key,
            displayName = model.DisplayName,
            description = model.Description,
            schema
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to create activity type. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        TempData["Success"] = "Activity type created.";

        return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateActivityType(UpdateActivityTypeViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        JsonElement? schema = null;
        if (!string.IsNullOrWhiteSpace(model.SchemaJson))
        {
            try
            {
                schema = JsonSerializer.Deserialize<JsonElement>(model.SchemaJson);
            }
            catch
            {
                TempData["Error"] = "Schema JSON is not valid JSON. Please fix it and try again.";
                return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
            }
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/activity-types/{model.Id}", new
        {
            key = model.Key,
            displayName = model.DisplayName,
            description = model.Description,
            schema
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update activity type. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        TempData["Success"] = "Activity type updated.";

        return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActivityType([FromForm] Guid id, [FromForm] string? language, [FromForm] Guid? levelId, [FromForm] Guid? moduleId, [FromForm] Guid? lessonId, [FromForm] Guid? quizId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/activity-types/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete activity type. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, quizId);
        }

        TempData["Success"] = "Activity type deleted.";
        return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, quizId);
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
        using var resp = await client.PostAsJsonAsync("/api/content/modules", new { levelId = model.LevelId, name = model.Name, orderIndex = model.OrderIndex }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", new { levelId = model.LevelId });
        }

        return RedirectToAction("Index", new { levelId = model.LevelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModule(UpdateModuleViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a module name.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, null, null, null);
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/modules/{model.Id}", new { name = model.Name, orderIndex = model.OrderIndex }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update module. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.Id, null, null);
        }

        TempData["Success"] = "Module updated.";
        return RedirectToIndexWithContext(model.Language, model.LevelId, model.Id, null, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule([FromForm] Guid id, [FromForm] string? language, [FromForm] Guid levelId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/modules/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete module. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, id, null, null);
        }

        TempData["Success"] = "Module deleted.";
        return RedirectToIndexWithContext(language, levelId, null, null, null);
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
    public async Task<IActionResult> DeleteLesson([FromForm] Guid id, [FromForm] string? language, [FromForm] Guid? levelId, [FromForm] Guid moduleId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/lessons/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete lesson. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, moduleId, id, null);
        }

        TempData["Success"] = "Lesson deleted.";
        return RedirectToIndexWithContext(language, levelId, moduleId, null, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActivity(CreateActivityViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
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
                TempData["Error"] = "Payload JSON is not valid JSON. Please fix it and try again.";
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

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to create activity. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { lessonId = model.LessonId });
        }

        TempData["Success"] = "Activity created.";

        return RedirectToAction("Index", new { lessonId = model.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActivity([FromForm] Guid id, [FromForm] string? language, [FromForm] Guid? levelId, [FromForm] Guid? moduleId, [FromForm] Guid lessonId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/activities/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete activity. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, null);
        }

        TempData["Success"] = "Activity deleted.";
        return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateActivity(UpdateActivityViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, null);
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
                TempData["Error"] = "Payload JSON is not valid JSON. Please fix it and try again.";
                return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, null);
            }
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/activities/{model.Id}", new
        {
            type = model.Type,
            title = model.Title,
            imageUrl = model.ImageUrl,
            payload,
            orderIndex = model.OrderIndex
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update activity. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, null);
        }

        TempData["Success"] = "Activity updated.";
        return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, null);
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

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to create quiz. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { lessonId = model.LessonId });
        }

        TempData["Success"] = "Quiz created.";
        return RedirectToAction("Index", new { lessonId = model.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuiz(UpdateQuizViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a quiz title.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.Id);
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/quizzes/{model.Id}", new { title = model.Title }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update quiz. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.Id);
        }

        TempData["Success"] = "Quiz updated.";
        return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.Id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuiz([FromForm] Guid id, [FromForm] string? language, [FromForm] Guid? levelId, [FromForm] Guid? moduleId, [FromForm] Guid? lessonId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/quizzes/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete quiz. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, id);
        }

        TempData["Success"] = "Quiz deleted.";
        return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, null);
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

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to create question. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToAction("Index", new { quizId = model.QuizId });
        }

        TempData["Success"] = "Question created.";
        return RedirectToAction("Index", new { quizId = model.QuizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuizQuestion(UpdateQuizQuestionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PutAsJsonAsync($"/api/content/questions/{model.Id}", new
        {
            prompt = model.Prompt,
            optionA = model.OptionA,
            optionB = model.OptionB,
            optionC = model.OptionC,
            optionD = model.OptionD,
            correctOption = model.CorrectOption
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to update question. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
        }

        TempData["Success"] = "Question updated.";
        return RedirectToIndexWithContext(model.Language, model.LevelId, model.ModuleId, model.LessonId, model.QuizId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuizQuestion([FromForm] Guid id, [FromForm] Guid quizId, [FromForm] string? language, [FromForm] Guid? levelId, [FromForm] Guid? moduleId, [FromForm] Guid? lessonId, CancellationToken ct)
    {
        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.DeleteAsync($"/api/content/questions/{id}", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"Failed to delete question. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
            return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, quizId);
        }

        TempData["Success"] = "Question deleted.";
        return RedirectToIndexWithContext(language, levelId, moduleId, lessonId, quizId);
    }

    private sealed record LevelDto(Guid Id, string Language, string Name);
    private sealed record ModuleDto(Guid Id, Guid LevelId, string Name, int OrderIndex);
    private sealed record LessonDto(Guid Id, Guid ModuleId, string Title, string? Description, string? Content, int OrderIndex);
    private sealed record ActivityDto(Guid Id, Guid LessonId, string Type, string Title, string? ImageUrl, JsonElement? Payload, int OrderIndex);
    private sealed record ActivityTypeDto(Guid Id, string Key, string DisplayName, string? Description, JsonElement? Schema);
    private sealed record QuizDto(Guid Id, Guid LessonId, string Title);
    private sealed record QuizQuestionDto(Guid Id, Guid QuizId, string Prompt, string OptionA, string OptionB, string OptionC, string OptionD, string CorrectOption, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
