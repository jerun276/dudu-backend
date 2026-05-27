namespace CupidLearn.Domain.Progress;

public class LessonProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? ChildId { get; set; }
    public Guid LessonId { get; set; }
    public bool Completed { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Attempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? LessonId { get; set; }
    public Guid? ExerciseId { get; set; }
    public string AttemptType { get; set; } = "";

    public Guid? ChildId { get; set; }
    public Guid? ExamId { get; set; }

    public int? Score { get; set; }
    public bool? Success { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }

    public bool? IsPassed { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
