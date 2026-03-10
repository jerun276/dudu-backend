using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Profiles;
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
            Age = request.Age
        };

        db.ChildProfiles.Add(child);
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
        c.CreatedAt,
        c.UpdatedAt);
}
