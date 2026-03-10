using CupidLearn.Api.Data;
using CupidLearn.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CupidLearn.Api.Infrastructure.Seeding;

public class DatabaseSeeder(
    AppDbContext db,
    IOptions<AdminSeedOptions> adminSeedOptionsAccessor)
{
    private readonly AdminSeedOptions _adminSeedOptions = adminSeedOptionsAccessor.Value;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public async Task SeedAsync(CancellationToken ct)
    {
        if (!_adminSeedOptions.Enabled)
        {
            return;
        }

        var anyAdmin = await db.Users.AnyAsync(x => x.Role == "ADMIN", ct);
        if (anyAdmin)
        {
            return;
        }

        var email = _adminSeedOptions.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(_adminSeedOptions.Password))
        {
            return;
        }

        var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existingUser != null)
        {
            existingUser.Role = "ADMIN";
            existingUser.IsVerified = true;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        var admin = new AppUser
        {
            Email = email,
            Role = "ADMIN",
            IsVerified = true
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, _adminSeedOptions.Password);

        db.Users.Add(admin);
        db.UserProfiles.Add(new UserProfile { UserId = admin.Id });

        await db.SaveChangesAsync(ct);
    }
}
