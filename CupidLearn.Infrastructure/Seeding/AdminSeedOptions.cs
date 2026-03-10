namespace CupidLearn.Infrastructure.Seeding;

public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public bool Enabled { get; set; } = true;

    public string Email { get; set; } = "admin@cupid.local";

    public string Password { get; set; } = "AdminPassword123!";
}
