using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Content;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Content;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ContentService(AppDbContext db) : IContentService
{
    public async Task<List<LevelResponse>> ListLevelsAsync(CancellationToken ct)
    {
        var levels = await db.Levels
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return levels.Select(ToResponse).ToList();
    }

    public async Task<List<ModuleResponse>> ListModulesByLevelAsync(Guid levelId, CancellationToken ct)
    {
        var levelExists = await db.Levels.AnyAsync(x => x.Id == levelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException("Level not found");
        }

        var modules = await db.Modules
            .Where(x => x.LevelId == levelId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return modules.Select(ToResponse).ToList();
    }

    public async Task<List<ExamResponse>> ListExamsByModuleAsync(Guid moduleId, CancellationToken ct)
    {
        var moduleExists = await db.Modules.AnyAsync(x => x.Id == moduleId, ct);
        if (!moduleExists)
        {
            throw new NotFoundException("Module not found");
        }

        var exams = await db.Exams
            .Where(x => x.ModuleId == moduleId)
            .OrderBy(x => x.Title)
            .ToListAsync(ct);

        return exams.Select(ToResponse).ToList();
    }

    public async Task<LevelResponse> CreateLevelAsync(LevelCreateRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        var level = new Level
        {
            Language = request.Language?.Trim(),
            Name = name
        };

        db.Levels.Add(level);
        await db.SaveChangesAsync(ct);

        return ToResponse(level);
    }

    public async Task<LevelResponse> UpdateLevelAsync(Guid levelId, LevelUpdateRequest request, CancellationToken ct)
    {
        var level = await db.Levels.FirstOrDefaultAsync(x => x.Id == levelId, ct);
        if (level == null)
        {
            throw new NotFoundException("Level not found");
        }

        level.Language = request.Language?.Trim();
        level.Name = request.Name.Trim();

        await db.SaveChangesAsync(ct);

        return ToResponse(level);
    }

    public async Task DeleteLevelAsync(Guid levelId, CancellationToken ct)
    {
        var level = await db.Levels.FirstOrDefaultAsync(x => x.Id == levelId, ct);
        if (level == null)
        {
            return;
        }

        var modules = await db.Modules.Where(x => x.LevelId == levelId).ToListAsync(ct);
        var moduleIds = modules.Select(x => x.Id).ToList();

        var lessons = await db.Lessons.Where(x => moduleIds.Contains(x.ModuleId)).ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();

        var activities = await db.LessonActivities.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);

        var quizzes = await db.Quizzes.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);
        var quizIds = quizzes.Select(x => x.Id).ToList();
        var questions = await db.QuizQuestions.Where(x => quizIds.Contains(x.QuizId)).ToListAsync(ct);

        db.QuizQuestions.RemoveRange(questions);
        db.Quizzes.RemoveRange(quizzes);
        db.LessonActivities.RemoveRange(activities);
        db.Lessons.RemoveRange(lessons);
        db.Modules.RemoveRange(modules);
        db.Levels.Remove(level);

        await db.SaveChangesAsync(ct);
    }

    public async Task<ModuleResponse> CreateModuleAsync(Guid levelId, ModuleCreateRequest request, CancellationToken ct)
    {
        var levelExists = await db.Levels.AnyAsync(x => x.Id == levelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException("Level not found");
        }

        var module = new Module
        {
            LevelId = levelId,
            Name = request.Name.Trim()
        };

        db.Modules.Add(module);
        await db.SaveChangesAsync(ct);

        return ToResponse(module);
    }

    public async Task<ExamResponse> CreateExamAsync(Guid moduleId, ExamCreateRequest request, CancellationToken ct)
    {
        var moduleExists = await db.Modules.AnyAsync(x => x.Id == moduleId, ct);
        if (!moduleExists)
        {
            throw new NotFoundException("Module not found");
        }

        var exam = new Exam
        {
            ModuleId = moduleId,
            Title = request.Title.Trim()
        };

        db.Exams.Add(exam);
        await db.SaveChangesAsync(ct);

        return ToResponse(exam);
    }

    private static LevelResponse ToResponse(Level x) => new(x.Id, x.Language, x.Name);

    private static ModuleResponse ToResponse(Module x) => new(x.Id, x.LevelId, x.Name);

    private static ExamResponse ToResponse(Exam x) => new(x.Id, x.ModuleId, x.Title);
}
