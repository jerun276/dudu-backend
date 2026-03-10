using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Application.Contracts.Progress;

public record AttemptCreateRequest(
    [Required] Guid LessonId,
    Guid? ExerciseId,
    [Required] string AttemptType,
    int? Score,
    bool? Success,
    [Required] string IdempotencyKey,
    [Required] DateTimeOffset OccurredAt);

public record AttemptResponse(
    Guid Id,
    Guid UserId,
    Guid LessonId,
    Guid? ExerciseId,
    string AttemptType,
    int? Score,
    bool? Success,
    string IdempotencyKey,
    DateTimeOffset OccurredAt);

public record LessonCompleteRequest([Required] DateTimeOffset CompletedAt);

public record LessonProgressResponse(
    Guid LessonId,
    string Status,
    DateTimeOffset? CompletedAt);

public record ProgressSummaryResponse(long CompletedLessons, long TotalAttempts);

public record ExamAttemptResponse(
    Guid Id,
    Guid ChildId,
    Guid ExamId,
    int Score,
    bool IsPassed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
