namespace CupidLearn.Api.Domain;

public class UserOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string OtpHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsVerified { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
