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
    public async Task<ActivityTypeResponse> CreateActivityTypeAsync(ActivityTypeCreateRequest request, CancellationToken ct)
    {
        var key = request.Key.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BadRequestException("Key is required");
        }

        var exists = await db.ActivityTypes.AnyAsync(x => x.Key == key, ct);
        if (exists)
        {
            throw new ConflictException("Activity type key already exists");
        }

        var now = DateTimeOffset.UtcNow;
        var type = new ActivityType
        {
            Key = key,
            DisplayName = request.DisplayName.Trim(),
            Description = request.Description,
            SchemaJson = request.Schema.HasValue ? JsonSerializer.Serialize(request.Schema.Value) : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync(ct);

        return ToActivityTypeResponse(type);
    }

    public async Task<ActivityTypeResponse> UpdateActivityTypeAsync(Guid activityTypeId, ActivityTypeUpdateRequest request, CancellationToken ct)
    {
        var type = await db.ActivityTypes.FirstOrDefaultAsync(x => x.Id == activityTypeId, ct);
        if (type == null)
        {
            throw new NotFoundException("Activity type not found");
        }

        var key = request.Key.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BadRequestException("Key is required");
        }

        var keyTaken = await db.ActivityTypes.AnyAsync(x => x.Id != activityTypeId && x.Key == key, ct);
        if (keyTaken)
        {
            throw new ConflictException("Activity type key already exists");
        }

        type.Key = key;
        type.DisplayName = request.DisplayName.Trim();
        type.Description = request.Description;
        type.SchemaJson = request.Schema.HasValue ? JsonSerializer.Serialize(request.Schema.Value) : null;
        type.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToActivityTypeResponse(type);
    }

    public async Task DeleteActivityTypeAsync(Guid activityTypeId, CancellationToken ct)
    {
        var type = await db.ActivityTypes.FirstOrDefaultAsync(x => x.Id == activityTypeId, ct);
        if (type == null)
        {
            return;
        }

        db.ActivityTypes.Remove(type);
        await db.SaveChangesAsync(ct);
    }

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
            Name = request.Name.Trim(),
            OrderIndex = request.OrderIndex
        };

        db.Modules.Add(module);
        await db.SaveChangesAsync(ct);

        return new ModuleResponse(module.Id, module.LevelId, module.Name, module.OrderIndex);
    }

    public async Task<ModuleResponse> UpdateModuleAsync(Guid moduleId, ModuleUpdateRequest request, CancellationToken ct)
    {
        var module = await db.Modules.FirstOrDefaultAsync(x => x.Id == moduleId, ct);
        if (module == null)
        {
            throw new NotFoundException("Module not found");
        }

        module.Name = request.Name.Trim();
        module.OrderIndex = request.OrderIndex;
        await db.SaveChangesAsync(ct);

        return new ModuleResponse(module.Id, module.LevelId, module.Name, module.OrderIndex);
    }

    public async Task DeleteModuleAsync(Guid moduleId, CancellationToken ct)
    {
        var module = await db.Modules.FirstOrDefaultAsync(x => x.Id == moduleId, ct);
        if (module == null)
        {
            return;
        }

        var lessons = await db.Lessons.Where(x => x.ModuleId == moduleId).ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();

        var activities = await db.LessonActivities.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);

        var quizzes = await db.Quizzes.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);
        var quizIds = quizzes.Select(x => x.Id).ToList();
        var questions = await db.QuizQuestions.Where(x => quizIds.Contains(x.QuizId)).ToListAsync(ct);

        db.QuizQuestions.RemoveRange(questions);
        db.Quizzes.RemoveRange(quizzes);
        db.LessonActivities.RemoveRange(activities);
        db.Lessons.RemoveRange(lessons);
        db.Modules.Remove(module);

        await db.SaveChangesAsync(ct);
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

    public async Task<QuizResponse> UpdateQuizAsync(Guid quizId, QuizUpdateRequest request, CancellationToken ct)
    {
        var quiz = await db.Quizzes.FirstOrDefaultAsync(x => x.Id == quizId, ct);
        if (quiz == null)
        {
            throw new NotFoundException("Quiz not found");
        }

        quiz.Title = request.Title.Trim();
        await db.SaveChangesAsync(ct);

        return new QuizResponse(quiz.Id, quiz.LessonId, quiz.Title);
    }

    public async Task DeleteQuizAsync(Guid quizId, CancellationToken ct)
    {
        var quiz = await db.Quizzes.FirstOrDefaultAsync(x => x.Id == quizId, ct);
        if (quiz == null)
        {
            return;
        }

        var questions = await db.QuizQuestions.Where(x => x.QuizId == quizId).ToListAsync(ct);
        db.QuizQuestions.RemoveRange(questions);
        db.Quizzes.Remove(quiz);

        await db.SaveChangesAsync(ct);
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

    public async Task<QuizQuestionResponse> UpdateQuizQuestionAsync(Guid questionId, QuizQuestionUpdateRequest request, CancellationToken ct)
    {
        var q = await db.QuizQuestions.FirstOrDefaultAsync(x => x.Id == questionId, ct);
        if (q == null)
        {
            throw new NotFoundException("Quiz question not found");
        }

        var correct = request.CorrectOption.Trim().ToUpperInvariant();
        if (correct is not ("A" or "B" or "C" or "D"))
        {
            throw new BadRequestException("correctOption must be A, B, C, or D");
        }

        q.Prompt = request.Prompt.Trim();
        q.OptionA = request.OptionA.Trim();
        q.OptionB = request.OptionB.Trim();
        q.OptionC = request.OptionC.Trim();
        q.OptionD = request.OptionD.Trim();
        q.CorrectOption = correct;
        q.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return new QuizQuestionResponse(q.Id, q.QuizId, q.Prompt, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption, q.CreatedAt, q.UpdatedAt);
    }

    public async Task DeleteQuizQuestionAsync(Guid questionId, CancellationToken ct)
    {
        var q = await db.QuizQuestions.FirstOrDefaultAsync(x => x.Id == questionId, ct);
        if (q == null)
        {
            return;
        }

        db.QuizQuestions.Remove(q);
        await db.SaveChangesAsync(ct);
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

    private static ActivityTypeResponse ToActivityTypeResponse(ActivityType x)
    {
        JsonElement? schema = null;
        if (!string.IsNullOrWhiteSpace(x.SchemaJson))
        {
            schema = JsonSerializer.Deserialize<JsonElement>(x.SchemaJson);
        }

        return new ActivityTypeResponse(
            x.Id,
            x.Key,
            x.DisplayName,
            x.Description,
            schema,
            x.CreatedAt,
            x.UpdatedAt);
    }
}
