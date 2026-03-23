using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Admin;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Billing;
using CupidLearn.Domain.Profiles;
using CupidLearn.Domain.Users;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class AdminUsersService(AppDbContext db) : IAdminUsersService
{
    public async Task<AdminUserSearchResponse> SearchAsync(Guid authUserId, string? authRole, string? query, int skip, int take, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        var q = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(u =>
                u.Email.Contains(term) ||
                (u.PhoneNo != null && u.PhoneNo.Contains(term))
            );
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.PhoneNo,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync(ct);

        var userIds = items.Select(x => x.Id).ToList();

        var names = await db.UserProfiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .Select(p => new { p.UserId, p.FullName })
            .ToListAsync(ct);

        var nameMap = names
            .Where(x => !string.IsNullOrWhiteSpace(x.FullName))
            .ToDictionary(x => x.UserId, x => x.FullName);

        var respItems = items.Select(x => new AdminUserListItemResponse(
            x.Id,
            x.Email,
            x.PhoneNo,
            x.Role,
            nameMap.TryGetValue(x.Id, out var fullName) ? fullName : null,
            x.CreatedAt)).ToList();

        return new AdminUserSearchResponse(total, respItems);
    }

    public async Task<AdminUserSummaryResponse> GetSummaryAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var profile = await db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);

        var children = await db.ChildProfiles
            .AsNoTracking()
            .Where(x => x.ParentUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminChildResponse(x.Id, x.DisplayName, x.Age, x.CreatedAt))
            .ToListAsync(ct);

        var seats = await db.Seats
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new AdminSeatAssignmentResponse(x.Id, x.OrganizationId, x.Status.ToString()))
            .ToListAsync(ct);

        var subscription = await db.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        var limits = ComputeLimits(subscription?.Status);

        AdminSubscriptionResponse? subResp = null;
        if (subscription != null)
        {
            subResp = new AdminSubscriptionResponse(
                subscription.Id,
                subscription.Provider,
                subscription.ProviderSubscriptionId,
                subscription.Status.ToString(),
                subscription.CurrentPeriodStart,
                subscription.CurrentPeriodEnd,
                subscription.UpdatedAt);
        }

        return new AdminUserSummaryResponse(
            user.Id,
            user.Email,
            user.PhoneNo,
            user.Role,
            profile?.FullName,
            profile?.DisplayName,
            user.CreatedAt,
            profile?.UpdatedAt,
            subResp,
            limits,
            seats,
            children);
    }

    public async Task<AdminChildResponse> CreateChildAsync(Guid authUserId, string? authRole, Guid parentUserId, AdminChildCreateRequest request, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var parentExists = await db.Users.AnyAsync(x => x.Id == parentUserId, ct);
        if (!parentExists)
        {
            throw new NotFoundException("Parent user not found");
        }

        var child = new ChildProfile
        {
            ParentUserId = parentUserId,
            DisplayName = request.DisplayName,
            Age = request.Age
        };

        db.ChildProfiles.Add(child);
        await db.SaveChangesAsync(ct);

        return new AdminChildResponse(child.Id, child.DisplayName, child.Age, child.CreatedAt);
    }

    public async Task<AdminChildResponse> UpdateChildAsync(Guid authUserId, string? authRole, Guid childId, AdminChildUpdateRequest request, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var child = await db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == childId, ct);
        if (child == null)
        {
            throw new NotFoundException("Child not found");
        }

        child.DisplayName = request.DisplayName;
        child.Age = request.Age;
        child.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return new AdminChildResponse(child.Id, child.DisplayName, child.Age, child.CreatedAt);
    }

    public async Task DeleteChildAsync(Guid authUserId, string? authRole, Guid childId, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var child = await db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == childId, ct);
        if (child == null)
        {
            throw new NotFoundException("Child not found");
        }

        db.ChildProfiles.Remove(child);
        await db.SaveChangesAsync(ct);
    }

    private static AdminSubscriptionLimitsResponse ComputeLimits(SubscriptionStatus? status)
    {
        var paid = status is SubscriptionStatus.ACTIVE or SubscriptionStatus.TRIALING;
        if (paid)
        {
            return new AdminSubscriptionLimitsResponse("PRO", 10, 100, 10);
        }

        return new AdminSubscriptionLimitsResponse("FREE", 1, 5, 0);
    }

    private static void EnsureAdmin(string? authRole)
    {
        if (!string.Equals(authRole, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Forbidden");
        }
    }
}
