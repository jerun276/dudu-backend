namespace CupidLearn.Domain.Content;

public class Level
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Language { get; set; }
    public string? Name { get; set; }
}

public class Module
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LevelId { get; set; }
    public string? Name { get; set; }
}

public class Exam
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public string? Title { get; set; }
}

public class ActivityType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Description { get; set; }

    public string? SchemaJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
