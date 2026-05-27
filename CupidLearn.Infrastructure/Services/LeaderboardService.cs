using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class LeaderboardService(AppDbContext db) : ILeaderboardService
{
    public async Task<List<LeaderboardEntryResponse>> GetGlobalLeaderboardAsync(Guid userId, Guid childId, int top, CancellationToken ct)
    {
        var entries = await db.CoinTransactions
            .GroupBy(x => x.ChildId)
            .Select(g => new { ChildId = g.Key, Coins = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Coins)
            .Take(top)
            .ToListAsync(ct);

        var childIds = entries.Select(e => e.ChildId).ToList();
        var profiles = await db.ChildProfiles
            .AsNoTracking()
            .Where(c => childIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var result = entries.Select((e, i) => new LeaderboardEntryResponse(
            i + 1,
            e.ChildId,
            profiles.TryGetValue(e.ChildId, out var p) ? p.DisplayName : "Unknown",
            profiles.TryGetValue(e.ChildId, out var p2) ? p2.AvatarUrl : null,
            e.Coins
        )).ToList();

        // If the current child isn't in the top list, append their entry
        if (!result.Any(r => r.ChildId == childId))
        {
            var myCoins = await db.CoinTransactions
                .Where(x => x.ChildId == childId)
                .SumAsync(x => x.Amount, ct);

            var myRank = await db.CoinTransactions
                .GroupBy(x => x.ChildId)
                .Select(g => g.Sum(x => x.Amount))
                .CountAsync(s => s > myCoins, ct) + 1;

            var myProfile = await db.ChildProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == childId, ct);

            result.Add(new LeaderboardEntryResponse(
                myRank,
                childId,
                myProfile?.DisplayName ?? "You",
                myProfile?.AvatarUrl,
                myCoins
            ));
        }

        return result;
    }
}
