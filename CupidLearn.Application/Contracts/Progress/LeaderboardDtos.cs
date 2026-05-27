namespace CupidLearn.Application.Contracts.Progress;

public record LeaderboardEntryResponse(
    int Rank,
    Guid ChildId,
    string DisplayName,
    string? AvatarUrl,
    int Coins);
