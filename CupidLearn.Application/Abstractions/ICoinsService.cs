using CupidLearn.Application.Contracts.Progress;

namespace CupidLearn.Application.Abstractions;

public interface ICoinsService
{
    Task<CoinBalanceResponse> GetBalanceAsync(Guid userId, Guid childId, CancellationToken ct);
    Task<CoinTransactionResponse> AwardCoinsAsync(Guid userId, Guid childId, int amount, string reason, Guid? referenceId, CancellationToken ct);
}
