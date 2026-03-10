using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface IExamAttemptService
{
    Task<ExamAttemptResponse?> GetAsync(Guid authUserId, Guid childId, Guid examId, CancellationToken ct);

    Task<bool> CanAttemptAsync(Guid authUserId, Guid childId, Guid examId, CancellationToken ct);

    Task<bool> HasPassedAsync(Guid authUserId, Guid childId, Guid examId, int passingScore, CancellationToken ct);

    Task<ExamAttemptResponse> RecordAttemptAsync(Guid authUserId, Guid childId, Guid examId, int score, int passingScore, CancellationToken ct);
}
