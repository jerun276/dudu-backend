using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Progress;
using CupidLearn.Domain.Progress;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class BadgeService(AppDbContext db) : IBadgeService
{
    private static readonly List<(string Key, string DisplayName, string Description, string Icon)> Definitions =
    [
        ("first_lesson", "First Steps", "Complete your first lesson", "school"),
        ("five_lessons", "Quick Learner", "Complete 5 lessons", "book"),
        ("ten_lessons", "Knowledge Seeker", "Complete 10 lessons", "library"),
        ("first_quiz", "Quiz Whiz", "Pass your first quiz", "checkmark-circle"),
        ("coin_collector", "Coin Collector", "Earn 100 coins", "cash"),
        ("coin_master", "Coin Master", "Earn 500 coins", "trophy"),
    ];

    public async Task<List<BadgeResponse>> ListBadgesAsync(Guid userId, Guid childId, CancellationToken ct)
    {
        await EnsureBadgeDefinitionsExist(ct);

        var definitions = await db.BadgeDefinitions.AsNoTracking().ToListAsync(ct);
        var earned = await db.EarnedBadges
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .ToListAsync(ct);

        var earnedMap = earned.ToDictionary(x => x.BadgeId, x => x.EarnedAt);

        return definitions.Select(d => new BadgeResponse(
            d.Id,
            d.Key,
            d.DisplayName,
            d.Description,
            d.Icon,
            earnedMap.ContainsKey(d.Id),
            earnedMap.GetValueOrDefault(d.Id)
        )).ToList();
    }

    public async Task EvaluateAndAwardAsync(Guid userId, Guid childId, CancellationToken ct)
    {
        await EnsureBadgeDefinitionsExist(ct);

        var definitions = await db.BadgeDefinitions.AsNoTracking().ToListAsync(ct);
        var earned = await db.EarnedBadges
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .Select(x => x.BadgeId)
            .ToListAsync(ct);

        var earnedSet = earned.ToHashSet();

        var completedLessons = await db.LessonProgress
            .LongCountAsync(x => x.UserId == userId && x.ChildId == childId && x.Completed, ct);

        var passedQuizzes = await db.Attempts
            .LongCountAsync(x => x.UserId == userId && x.ChildId == childId && x.AttemptType == "EXAM" && x.IsPassed == true, ct);

        var totalCoins = await db.CoinTransactions
            .Where(x => x.UserId == userId && x.ChildId == childId)
            .SumAsync(x => x.Amount, ct);

        foreach (var def in definitions)
        {
            if (earnedSet.Contains(def.Id)) continue;

            var shouldAward = def.Key switch
            {
                "first_lesson" => completedLessons >= 1,
                "five_lessons" => completedLessons >= 5,
                "ten_lessons" => completedLessons >= 10,
                "first_quiz" => passedQuizzes >= 1,
                "coin_collector" => totalCoins >= 100,
                "coin_master" => totalCoins >= 500,
                _ => false
            };

            if (shouldAward)
            {
                db.EarnedBadges.Add(new EarnedBadge
                {
                    UserId = userId,
                    ChildId = childId,
                    BadgeId = def.Id
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureBadgeDefinitionsExist(CancellationToken ct)
    {
        var existingKeys = await db.BadgeDefinitions.Select(x => x.Key).ToListAsync(ct);
        var existingSet = existingKeys.ToHashSet();

        foreach (var (key, displayName, description, icon) in Definitions)
        {
            if (existingSet.Contains(key)) continue;
            db.BadgeDefinitions.Add(new BadgeDefinition
            {
                Key = key,
                DisplayName = displayName,
                Description = description,
                Icon = icon
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
