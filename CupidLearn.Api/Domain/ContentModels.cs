namespace CupidLearn.Api.Domain;

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
