using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface IProgressService
{
    Task<AttemptResponse> RecordAttemptAsync(Guid userId, AttemptCreateRequest request, CancellationToken ct);

    Task<LessonProgressResponse> CompleteLessonAsync(Guid userId, Guid lessonId, LessonCompleteRequest request, CancellationToken ct);

    Task<List<LessonProgressResponse>> ListLessonProgressAsync(Guid userId, CancellationToken ct);

    Task<ProgressSummaryResponse> SummaryAsync(Guid userId, CancellationToken ct);
}
