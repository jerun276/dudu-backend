using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Progress;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ExamAttemptService(AppDbContext db, ICoinsService coinsService) : IExamAttemptService
{
    public async Task<ExamAttemptResponse?> GetAsync(Guid authUserId, Guid childId, Guid examId, CancellationToken ct)
    {
        await EnsureChildOwnedByUser(authUserId, childId, ct);

        var attempt = await db.Attempts
            .AsNoTracking()
            .Where(x => x.ChildId == childId && x.ExamId == examId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return attempt == null ? null : ToResponse(attempt);
    }

    public async Task<bool> CanAttemptAsync(Guid authUserId, Guid childId, Guid examId, CancellationToken ct)
    {
        await EnsureChildOwnedByUser(authUserId, childId, ct);

        var exists = await db.Attempts.AnyAsync(x => x.ChildId == childId && x.ExamId == examId, ct);
        return !exists;
    }

    public async Task<bool> HasPassedAsync(Guid authUserId, Guid childId, Guid examId, int passingScore, CancellationToken ct)
    {
        await EnsureChildOwnedByUser(authUserId, childId, ct);

        var attempt = await db.Attempts
            .AsNoTracking()
            .Where(x => x.ChildId == childId && x.ExamId == examId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (attempt == null)
        {
            return false;
        }

        var score = attempt.Score ?? 0;
        return score >= passingScore;
    }

    public async Task<ExamAttemptResponse> RecordAttemptAsync(Guid authUserId, Guid childId, Guid examId, int score, int passingScore, CancellationToken ct)
    {
        await EnsureChildOwnedByUser(authUserId, childId, ct);

        var exists = await db.Attempts.AnyAsync(x => x.ChildId == childId && x.ExamId == examId, ct);
        if (exists)
        {
            throw new BadRequestException("Exam already attempted by this child");
        }

        var isPassed = score >= passingScore;

        var attempt = new Attempt
        {
            UserId = authUserId,
            ChildId = childId,
            ExamId = examId,
            AttemptType = "EXAM",
            Score = score,
            Success = isPassed,
            IsPassed = isPassed,
            OccurredAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Attempts.Add(attempt);
        await db.SaveChangesAsync(ct);

        if (isPassed)
        {
            await coinsService.AwardCoinsAsync(authUserId, childId, 25, "quiz_pass", examId, ct);
        }

        return ToResponse(attempt);
    }

    private async Task EnsureChildOwnedByUser(Guid authUserId, Guid childId, CancellationToken ct)
    {
        var owned = await db.ChildProfiles.AnyAsync(x => x.Id == childId && x.ParentUserId == authUserId, ct);
        if (!owned)
        {
            throw new ForbiddenException("Forbidden");
        }
    }

    private static ExamAttemptResponse ToResponse(Attempt a) => new(
        a.Id,
        a.ChildId ?? Guid.Empty,
        a.ExamId ?? Guid.Empty,
        a.Score ?? 0,
        a.IsPassed ?? (a.Success ?? false),
        a.CreatedAt,
        a.UpdatedAt);
}
