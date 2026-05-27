using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Profiles;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Profiles;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class ChildrenService(AppDbContext db) : IChildrenService
{
    public async Task<ChildProfileResponse> CreateAsync(Guid parentUserId, ChildProfileCreateRequest request, CancellationToken ct)
    {
        var child = new ChildProfile
        {
            ParentUserId = parentUserId,
            DisplayName = request.DisplayName,
            Age = request.Age,
            AvatarUrl = request.AvatarUrl
        };

        db.ChildProfiles.Add(child);
        await db.SaveChangesAsync(ct);

        return ToResponse(child);
    }

    public async Task<ChildProfileResponse> UpdateAsync(Guid parentUserId, Guid childId, ChildProfileUpdateRequest request, CancellationToken ct)
    {
        var child = await db.ChildProfiles
            .FirstOrDefaultAsync(x => x.Id == childId && x.ParentUserId == parentUserId, ct);

        if (child == null)
            throw new NotFoundException("Child profile not found");

        if (request.DisplayName != null) child.DisplayName = request.DisplayName;
        if (request.Age.HasValue) child.Age = request.Age;
        if (request.AvatarUrl != null) child.AvatarUrl = request.AvatarUrl;
        child.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToResponse(child);
    }

    public async Task<List<ChildProfileResponse>> ListAsync(Guid parentUserId, CancellationToken ct)
    {
        var children = await db.ChildProfiles
            .Where(x => x.ParentUserId == parentUserId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return children.Select(ToResponse).ToList();
    }

    private static ChildProfileResponse ToResponse(ChildProfile c) => new(
        c.Id,
        c.ParentUserId,
        c.DisplayName,
        c.Age,
        c.AvatarUrl,
        c.CreatedAt,
        c.UpdatedAt);
}
