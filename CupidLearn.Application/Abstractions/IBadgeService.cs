using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface IBadgeService
{
    Task<List<BadgeResponse>> ListBadgesAsync(Guid userId, Guid childId, CancellationToken ct);
    Task EvaluateAndAwardAsync(Guid userId, Guid childId, CancellationToken ct);
}
