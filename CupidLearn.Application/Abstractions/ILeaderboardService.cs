using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntryResponse>> GetGlobalLeaderboardAsync(Guid userId, Guid childId, int top, CancellationToken ct);
}
