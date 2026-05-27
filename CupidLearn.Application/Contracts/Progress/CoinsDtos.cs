namespace CupidLearn.Application.Contracts.Progress;

public record CoinBalanceResponse(int Balance, List<CoinTransactionResponse> RecentTransactions);

public record CoinTransactionResponse(
    Guid Id,
    int Amount,
    string Reason,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt);
