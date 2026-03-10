namespace CupidLearn.Api.Domain;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Locale { get; set; }

    public string? Country { get; set; }

    public string? Province { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
