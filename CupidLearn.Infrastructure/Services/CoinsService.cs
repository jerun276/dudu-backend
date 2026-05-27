using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Domain.Progress;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class CoinsService(AppDbContext db) : ICoinsService
{
    public async Task<CoinBalanceResponse> GetBalanceAsync(Guid userId, Guid childId, CancellationToken ct)
    {
        var balance = await db.CoinTransactions
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .SumAsync(x => x.Amount, ct);

        var recent = await db.CoinTransactions
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new CoinTransactionResponse(x.Id, x.Amount, x.Reason, x.ReferenceId, x.CreatedAt))
            .ToListAsync(ct);

        return new CoinBalanceResponse(balance, recent);
    }

    public async Task<CoinTransactionResponse> AwardCoinsAsync(Guid userId, Guid childId, int amount, string reason, Guid? referenceId, CancellationToken ct)
    {
        var tx = new CoinTransaction
        {
            UserId = userId,
            ChildId = childId,
            Amount = amount,
            Reason = reason,
            ReferenceId = referenceId
        };

        db.CoinTransactions.Add(tx);
        await db.SaveChangesAsync(ct);

        return new CoinTransactionResponse(tx.Id, tx.Amount, tx.Reason, tx.ReferenceId, tx.CreatedAt);
    }
}
