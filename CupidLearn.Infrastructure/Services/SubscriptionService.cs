using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Billing;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Billing;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class SubscriptionService(AppDbContext db) : ISubscriptionService
{
    public async Task<SubscriptionResponse> GetByUserIdAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct)
    {
        EnsureSelfOrAdmin(authUserId, authRole, userId);

        var sub = await db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (sub == null)
        {
            throw new NotFoundException("Subscription not found");
        }

        return ToResponse(sub);
    }

    public async Task<SubscriptionResponse> UpsertAsync(Guid authUserId, string? authRole, Guid userId, SubscriptionUpsertRequest request, CancellationToken ct)
    {
        var isAdmin = string.Equals(authRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            throw new ForbiddenException("Forbidden");
        }

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            throw new BadRequestException("provider is required");
        }

        if (string.IsNullOrWhiteSpace(request.ProviderSubscriptionId))
        {
            throw new BadRequestException("providerSubscriptionId is required");
        }

        var sub = await db.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (sub == null)
        {
            sub = new Subscription { UserId = userId };
            db.Subscriptions.Add(sub);
        }

        sub.Provider = request.Provider.Trim();
        sub.ProviderSubscriptionId = request.ProviderSubscriptionId.Trim();
        sub.Status = request.Status;
        sub.CurrentPeriodStart = request.CurrentPeriodStart;
        sub.CurrentPeriodEnd = request.CurrentPeriodEnd;
        sub.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToResponse(sub);
    }

    public async Task<SubscriptionLimitsResponse> GetLimitsAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct)
    {
        EnsureSelfOrAdmin(authUserId, authRole, userId);

        var sub = await db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        var status = sub?.Status;

        var paid = status is SubscriptionStatus.ACTIVE or SubscriptionStatus.TRIALING;

        if (paid)
        {
            return new SubscriptionLimitsResponse("PRO", 10, 100, 10);
        }

        return new SubscriptionLimitsResponse("FREE", 1, 5, 0);
    }

    private static SubscriptionResponse ToResponse(Subscription s) => new(
        s.Id,
        s.UserId,
        s.Provider,
        s.ProviderSubscriptionId,
        s.Status,
        s.CurrentPeriodStart,
        s.CurrentPeriodEnd,
        s.CreatedAt,
        s.UpdatedAt);

    private static void EnsureSelfOrAdmin(Guid authUserId, string? authRole, Guid userId)
    {
        var isAdmin = string.Equals(authRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && authUserId != userId)
        {
            throw new ForbiddenException("Forbidden");
        }
    }
}
