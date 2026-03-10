using System.Text.Json;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Content;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Content;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ContentQueryService(AppDbContext db) : IContentQueryService
{
    public async Task<List<LevelResponse>> ListLevelsAsync(string? language, CancellationToken ct)
    {
        var query = db.Levels.AsQueryable();
        if (!string.IsNullOrWhiteSpace(language))
        {
            var lang = language.Trim();
            query = query.Where(x => x.Language == lang);
        }

        var levels = await query.OrderBy(x => x.Name).ToListAsync(ct);
        return levels.Select(x => new LevelResponse(x.Id, x.Language, x.Name)).ToList();
    }

    public async Task<List<ModuleResponse>> ListModulesByLevelAsync(Guid levelId, CancellationToken ct)
    {
        var levelExists = await db.Levels.AnyAsync(x => x.Id == levelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException("Level not found");
        }

        var modules = await db.Modules.Where(x => x.LevelId == levelId).OrderBy(x => x.Name).ToListAsync(ct);
        return modules.Select(x => new ModuleResponse(x.Id, x.LevelId, x.Name)).ToList();
    }

    public async Task<List<LessonResponse>> ListLessonsByModuleAsync(Guid moduleId, CancellationToken ct)
    {
        var moduleExists = await db.Modules.AnyAsync(x => x.Id == moduleId, ct);
        if (!moduleExists)
        {
            throw new NotFoundException("Module not found");
        }

        var lessons = await db.Lessons.Where(x => x.ModuleId == moduleId).OrderBy(x => x.OrderIndex).ToListAsync(ct);
        return lessons.Select(ToLessonResponse).ToList();
    }

    public async Task<List<ActivityResponse>> ListActivitiesByLessonAsync(Guid lessonId, CancellationToken ct)
    {
        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == lessonId, ct);
        if (!lessonExists)
        {
            throw new NotFoundException("Lesson not found");
        }

        var activities = await db.LessonActivities.Where(x => x.LessonId == lessonId).OrderBy(x => x.OrderIndex).ToListAsync(ct);
        return activities.Select(ToActivityResponse).ToList();
    }

    public async Task<ActivityResponse> GetActivityByIdAsync(Guid activityId, CancellationToken ct)
    {
        var activity = await db.LessonActivities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == activityId, ct);
        if (activity == null)
        {
            throw new NotFoundException("Activity not found");
        }

        return ToActivityResponse(activity);
    }

    public async Task<List<QuizResponse>> ListQuizzesByLessonAsync(Guid lessonId, CancellationToken ct)
    {
        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == lessonId, ct);
        if (!lessonExists)
        {
            throw new NotFoundException("Lesson not found");
        }

        var quizzes = await db.Quizzes.Where(x => x.LessonId == lessonId).OrderBy(x => x.Title).ToListAsync(ct);
        return quizzes.Select(x => new QuizResponse(x.Id, x.LessonId, x.Title)).ToList();
    }

    public async Task<List<QuizQuestionResponse>> ListQuestionsByQuizAsync(Guid quizId, CancellationToken ct)
    {
        var quizExists = await db.Quizzes.AnyAsync(x => x.Id == quizId, ct);
        if (!quizExists)
        {
            throw new NotFoundException("Quiz not found");
        }

        var questions = await db.QuizQuestions.Where(x => x.QuizId == quizId).OrderBy(x => x.CreatedAt).ToListAsync(ct);
        return questions.Select(ToQuestionResponse).ToList();
    }

    public async Task<QuizResponse> GetQuizByIdAsync(Guid quizId, CancellationToken ct)
    {
        var quiz = await db.Quizzes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == quizId, ct);
        if (quiz == null)
        {
            throw new NotFoundException("Quiz not found");
        }

        return new QuizResponse(quiz.Id, quiz.LessonId, quiz.Title);
    }

    private static LessonResponse ToLessonResponse(Lesson x) => new(
        x.Id,
        x.ModuleId,
        x.Title,
        x.Description,
        x.Content,
        x.OrderIndex,
        x.CreatedAt,
        x.UpdatedAt);

    private static ActivityResponse ToActivityResponse(LessonActivity x)
    {
        JsonElement? payload = null;
        if (!string.IsNullOrWhiteSpace(x.PayloadJson))
        {
            payload = JsonSerializer.Deserialize<JsonElement>(x.PayloadJson);
        }

        return new ActivityResponse(
            x.Id,
            x.LessonId,
            x.Type,
            x.Title,
            x.ImageUrl,
            payload,
            x.OrderIndex,
            x.CreatedAt,
            x.UpdatedAt);
    }

    private static QuizQuestionResponse ToQuestionResponse(QuizQuestion x) => new(
        x.Id,
        x.QuizId,
        x.Prompt,
        x.OptionA,
        x.OptionB,
        x.OptionC,
        x.OptionD,
        x.CorrectOption,
        x.CreatedAt,
        x.UpdatedAt);
}
