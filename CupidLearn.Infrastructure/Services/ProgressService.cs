using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Progress;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ProgressService(AppDbContext db) : IProgressService
{
    public async Task<AttemptResponse> RecordAttemptAsync(Guid userId, Guid childId, AttemptCreateRequest request, CancellationToken ct)
    {
        var attemptType = request.AttemptType.Trim();
        if (string.IsNullOrWhiteSpace(attemptType))
        {
            throw new BadRequestException("attemptType is required");
        }

        var idempotencyKey = request.IdempotencyKey.Trim();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new BadRequestException("idempotencyKey is required");
        }

        var existing = await db.Attempts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ChildId == childId && x.IdempotencyKey == idempotencyKey, ct);

        if (existing != null)
        {
            return ToResponse(existing);
        }

        var attempt = new Attempt
        {
            UserId = userId,
            ChildId = childId,
            LessonId = request.LessonId,
            ExerciseId = request.ExerciseId,
            AttemptType = attemptType,
            Score = request.Score,
            Success = request.Success,
            IdempotencyKey = idempotencyKey,
            OccurredAt = request.OccurredAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Attempts.Add(attempt);
        await db.SaveChangesAsync(ct);

        return ToResponse(attempt);
    }

    public async Task<LessonProgressResponse> CompleteLessonAsync(Guid userId, Guid childId, Guid lessonId, LessonCompleteRequest request, CancellationToken ct)
    {
        var row = await db.LessonProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.ChildId == childId && x.LessonId == lessonId, ct);
        if (row == null)
        {
            row = new LessonProgress { UserId = userId, ChildId = childId, LessonId = lessonId };
            db.LessonProgress.Add(row);
        }

        row.Completed = true;
        row.UpdatedAt = request.CompletedAt;

        await db.SaveChangesAsync(ct);

        return new LessonProgressResponse(row.LessonId, "COMPLETED", row.UpdatedAt);
    }

    public async Task<List<LessonProgressResponse>> ListLessonProgressAsync(Guid userId, Guid childId, CancellationToken ct)
    {
        var rows = await db.LessonProgress
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);

        return rows
            .Select(x => new LessonProgressResponse(x.LessonId, x.Completed ? "COMPLETED" : "IN_PROGRESS", x.Completed ? x.UpdatedAt : null))
            .ToList();
    }

    public async Task<ProgressSummaryResponse> SummaryAsync(Guid userId, Guid childId, CancellationToken ct)
    {
        var completedLessons = await db.LessonProgress.LongCountAsync(x => x.UserId == userId && x.ChildId == childId && x.Completed, ct);
        var totalAttempts = await db.Attempts.LongCountAsync(x => x.UserId == userId && x.ChildId == childId, ct);

        return new ProgressSummaryResponse(completedLessons, totalAttempts);
    }

    private static AttemptResponse ToResponse(Attempt a) => new(
        a.Id,
        a.UserId,
        a.LessonId ?? Guid.Empty,
        a.ExerciseId,
        a.AttemptType,
        a.Score,
        a.Success,
        a.IdempotencyKey ?? string.Empty,
        a.OccurredAt ?? a.CreatedAt);
}
