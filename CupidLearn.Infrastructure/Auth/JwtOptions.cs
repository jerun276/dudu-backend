namespace CupidLearn.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public int ExpirationSeconds { get; set; } = 3600;

    public string Issuer { get; set; } = "cupid";

    public string Audience { get; set; } = "cupid";
}
