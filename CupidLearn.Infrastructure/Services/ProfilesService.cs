using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Profiles;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Profiles;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ProfilesService(AppDbContext db) : IProfilesService
{
    public async Task<ProfileResponse> GetByUserIdAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct)
    {
        var isAdmin = string.Equals(authRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && authUserId != userId)
        {
            throw new ForbiddenException("Forbidden");
        }

        var userExists = await db.Users.AnyAsync(x => x.Id == userId, ct);
        if (!userExists)
        {
            throw new NotFoundException("Profile not found");
        }

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(profile);
            await db.SaveChangesAsync(ct);
        }

        return ToResponse(profile);
    }

    public async Task<ProfileResponse> UpsertAsync(Guid authUserId, Guid userId, ProfileUpsertRequest request, CancellationToken ct)
    {
        var isAdmin = string.Equals(await db.Users.Where(x => x.Id == authUserId).Select(x => x.Role).FirstOrDefaultAsync(ct), "ADMIN", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && authUserId != userId)
        {
            throw new ForbiddenException("Forbidden");
        }

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(profile);
        }

        profile.DisplayName = request.DisplayName;
        profile.FullName = request.FullName;
        profile.AvatarUrl = request.AvatarUrl;
        profile.Locale = request.Locale;
        profile.Country = request.Country;
        profile.Province = request.Province;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToResponse(profile);
    }

    private static ProfileResponse ToResponse(UserProfile p) => new(
        p.Id,
        p.UserId,
        p.DisplayName,
        p.FullName,
        p.AvatarUrl,
        p.Locale,
        p.Country,
        p.Province,
        p.CreatedAt,
        p.UpdatedAt);
}
