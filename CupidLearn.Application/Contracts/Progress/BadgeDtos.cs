namespace CupidLearn.Application.Contracts.Progress;

public record BadgeResponse(
    Guid Id,
    string Key,
    string DisplayName,
    string Description,
    string Icon,
    bool Earned,
    DateTimeOffset? EarnedAt);
