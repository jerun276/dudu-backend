using CupidLearn.Application.Contracts.Billing;

namespace CupidLearn.Application.Abstractions;

public interface ISubscriptionService
{
    Task<SubscriptionResponse> GetByUserIdAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct);

    Task<SubscriptionResponse> UpsertAsync(Guid authUserId, string? authRole, Guid userId, SubscriptionUpsertRequest request, CancellationToken ct);

    Task<SubscriptionLimitsResponse> GetLimitsAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct);
}
