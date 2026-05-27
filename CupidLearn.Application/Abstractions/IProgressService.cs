using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface IProgressService
{
    Task<AttemptResponse> RecordAttemptAsync(Guid userId, Guid childId, AttemptCreateRequest request, CancellationToken ct);

    Task<LessonProgressResponse> CompleteLessonAsync(Guid userId, Guid childId, Guid lessonId, LessonCompleteRequest request, CancellationToken ct);

    Task<List<LessonProgressResponse>> ListLessonProgressAsync(Guid userId, Guid childId, CancellationToken ct);

    Task<ProgressSummaryResponse> SummaryAsync(Guid userId, Guid childId, CancellationToken ct);
}
