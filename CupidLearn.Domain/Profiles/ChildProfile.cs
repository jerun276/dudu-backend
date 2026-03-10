namespace CupidLearn.Domain.Profiles;

public class ChildProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ParentUserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int? Age { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
