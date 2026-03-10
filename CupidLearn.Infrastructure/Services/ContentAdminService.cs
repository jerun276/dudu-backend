using System.Text.Json;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Content;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Content;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ContentAdminService(AppDbContext db) : IContentAdminService
{
    public async Task<ModuleResponse> CreateModuleAsync(ModuleCreateV2Request request, CancellationToken ct)
    {
        var levelExists = await db.Levels.AnyAsync(x => x.Id == request.LevelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException("Level not found");
        }

        var module = new Module
        {
            LevelId = request.LevelId,
            Name = request.Name.Trim()
        };

        db.Modules.Add(module);
        await db.SaveChangesAsync(ct);

        return new ModuleResponse(module.Id, module.LevelId, module.Name);
    }

    public async Task<LessonResponse> CreateLessonAsync(Guid moduleId, LessonCreateRequest request, CancellationToken ct)
    {
        var moduleExists = await db.Modules.AnyAsync(x => x.Id == moduleId, ct);
        if (!moduleExists)
        {
            throw new NotFoundException("Module not found");
        }

        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Content = request.Content,
            OrderIndex = request.OrderIndex,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        return ToLessonResponse(lesson);
    }

    public async Task<LessonResponse> UpdateLessonAsync(Guid lessonId, LessonUpdateRequest request, CancellationToken ct)
    {
        var lesson = await db.Lessons.FirstOrDefaultAsync(x => x.Id == lessonId, ct);
        if (lesson == null)
        {
            throw new NotFoundException("Lesson not found");
        }

        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description;
        lesson.Content = request.Content;
        lesson.OrderIndex = request.OrderIndex;
        lesson.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToLessonResponse(lesson);
    }

    public async Task DeleteLessonAsync(Guid lessonId, CancellationToken ct)
    {
        var lesson = await db.Lessons.FirstOrDefaultAsync(x => x.Id == lessonId, ct);
        if (lesson == null)
        {
            return;
        }

        var activities = await db.LessonActivities.Where(x => x.LessonId == lessonId).ToListAsync(ct);
        db.LessonActivities.RemoveRange(activities);

        var quizzes = await db.Quizzes.Where(x => x.LessonId == lessonId).ToListAsync(ct);
        var quizIds = quizzes.Select(x => x.Id).ToList();
        var questions = await db.QuizQuestions.Where(x => quizIds.Contains(x.QuizId)).ToListAsync(ct);

        db.QuizQuestions.RemoveRange(questions);
        db.Quizzes.RemoveRange(quizzes);
        db.Lessons.Remove(lesson);

        await db.SaveChangesAsync(ct);
    }

    public async Task<ActivityResponse> CreateActivityAsync(Guid lessonId, ActivityCreateRequest request, CancellationToken ct)
    {
        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == lessonId, ct);
        if (!lessonExists)
        {
            throw new NotFoundException("Lesson not found");
        }

        var activity = new LessonActivity
        {
            LessonId = lessonId,
            Type = request.Type.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            ImageUrl = request.ImageUrl,
            PayloadJson = request.Payload.HasValue ? JsonSerializer.Serialize(request.Payload.Value) : null,
            OrderIndex = request.OrderIndex,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.LessonActivities.Add(activity);
        await db.SaveChangesAsync(ct);

        return ToActivityResponse(activity);
    }

    public async Task<ActivityResponse> UpdateActivityAsync(Guid activityId, ActivityUpdateRequest request, CancellationToken ct)
    {
        var activity = await db.LessonActivities.FirstOrDefaultAsync(x => x.Id == activityId, ct);
        if (activity == null)
        {
            throw new NotFoundException("Activity not found");
        }

        activity.Type = request.Type.Trim().ToUpperInvariant();
        activity.Title = request.Title.Trim();
        activity.ImageUrl = request.ImageUrl;
        activity.PayloadJson = request.Payload.HasValue ? JsonSerializer.Serialize(request.Payload.Value) : null;
        activity.OrderIndex = request.OrderIndex;
        activity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToActivityResponse(activity);
    }

    public async Task DeleteActivityAsync(Guid activityId, CancellationToken ct)
    {
        var activity = await db.LessonActivities.FirstOrDefaultAsync(x => x.Id == activityId, ct);
        if (activity == null)
        {
            return;
        }

        db.LessonActivities.Remove(activity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<QuizResponse> CreateQuizAsync(Guid lessonId, QuizCreateRequest request, CancellationToken ct)
    {
        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == lessonId, ct);
        if (!lessonExists)
        {
            throw new NotFoundException("Lesson not found");
        }

        var quiz = new Quiz
        {
            LessonId = lessonId,
            Title = request.Title.Trim()
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(ct);

        return new QuizResponse(quiz.Id, quiz.LessonId, quiz.Title);
    }

    public async Task<QuizQuestionResponse> CreateQuizQuestionAsync(Guid quizId, QuizQuestionCreateRequest request, CancellationToken ct)
    {
        var quizExists = await db.Quizzes.AnyAsync(x => x.Id == quizId, ct);
        if (!quizExists)
        {
            throw new NotFoundException("Quiz not found");
        }

        var correct = request.CorrectOption.Trim().ToUpperInvariant();
        if (correct is not ("A" or "B" or "C" or "D"))
        {
            throw new BadRequestException("correctOption must be A, B, C, or D");
        }

        var q = new QuizQuestion
        {
            QuizId = quizId,
            Prompt = request.Prompt.Trim(),
            OptionA = request.OptionA.Trim(),
            OptionB = request.OptionB.Trim(),
            OptionC = request.OptionC.Trim(),
            OptionD = request.OptionD.Trim(),
            CorrectOption = correct,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.QuizQuestions.Add(q);
        await db.SaveChangesAsync(ct);

        return new QuizQuestionResponse(q.Id, q.QuizId, q.Prompt, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption, q.CreatedAt, q.UpdatedAt);
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
}
