using Microsoft.EntityFrameworkCore;

using CupidLearn.Domain.Billing;
using CupidLearn.Domain.Content;
using CupidLearn.Domain.Progress;
using CupidLearn.Domain.Profiles;
using CupidLearn.Domain.Users;

namespace CupidLearn.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserOtp> UserOtps => Set<UserOtp>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();

    public DbSet<Level> Levels => Set<Level>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Exam> Exams => Set<Exam>();

    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();

    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonActivity> LessonActivities => Set<LessonActivity>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<CoinTransaction> CoinTransactions => Set<CoinTransaction>();
    public DbSet<BadgeDefinition> BadgeDefinitions => Set<BadgeDefinition>();
    public DbSet<EarnedBadge> EarnedBadges => Set<EarnedBadge>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Seat> Seats => Set<Seat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.PhoneNo).HasMaxLength(32);
            e.Property(x => x.Role).HasMaxLength(32);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
            e.Property(x => x.TokenHash).HasMaxLength(128);
            e.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        });

        modelBuilder.Entity<UserOtp>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.IsVerified });
            e.Property(x => x.OtpHash).HasMaxLength(512);
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.DisplayName).HasMaxLength(128);
            e.Property(x => x.FullName).HasMaxLength(256);
            e.Property(x => x.Country).HasMaxLength(128);
            e.Property(x => x.Province).HasMaxLength(128);
            e.Property(x => x.Locale).HasMaxLength(32);
        });

        modelBuilder.Entity<ChildProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ParentUserId);
            e.Property(x => x.DisplayName).HasMaxLength(128);
        });

        modelBuilder.Entity<Level>().HasKey(x => x.Id);
        modelBuilder.Entity<Module>().HasKey(x => x.Id);
        modelBuilder.Entity<Exam>().HasKey(x => x.Id);

        modelBuilder.Entity<ActivityType>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(128);
            e.Property(x => x.Description).HasMaxLength(1024);
        });

        modelBuilder.Entity<Lesson>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ModuleId);
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.Description).HasMaxLength(1024);
        });

        modelBuilder.Entity<LessonActivity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LessonId);
            e.Property(x => x.Type).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.ImageUrl).HasMaxLength(1024);
        });

        modelBuilder.Entity<Quiz>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LessonId);
            e.Property(x => x.Title).HasMaxLength(256);
        });

        modelBuilder.Entity<QuizQuestion>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.QuizId);
            e.Property(x => x.Prompt).HasMaxLength(2048);
            e.Property(x => x.OptionA).HasMaxLength(1024);
            e.Property(x => x.OptionB).HasMaxLength(1024);
            e.Property(x => x.OptionC).HasMaxLength(1024);
            e.Property(x => x.OptionD).HasMaxLength(1024);
            e.Property(x => x.CorrectOption).HasMaxLength(8);
        });

        modelBuilder.Entity<LessonProgress>().HasKey(x => x.Id);
        modelBuilder.Entity<LessonProgress>().HasIndex(x => new { x.ChildId, x.LessonId });

        modelBuilder.Entity<CoinTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.ChildId });
            e.Property(x => x.Reason).HasMaxLength(128);
        });

        modelBuilder.Entity<BadgeDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(64);
            e.Property(x => x.DisplayName).HasMaxLength(128);
            e.Property(x => x.Description).HasMaxLength(512);
            e.Property(x => x.Icon).HasMaxLength(64);
        });

        modelBuilder.Entity<EarnedBadge>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.ChildId, x.BadgeId }).IsUnique();
        });
        modelBuilder.Entity<Attempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AttemptType).HasMaxLength(64);
            e.Property(x => x.IdempotencyKey).HasMaxLength(128);
            e.HasIndex(x => new { x.UserId, x.IdempotencyKey }).IsUnique();
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.Provider).HasMaxLength(64);
            e.Property(x => x.ProviderSubscriptionId).HasMaxLength(128);
        });

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<Seat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrganizationId);
            e.HasIndex(x => new { x.OrganizationId, x.UserId });
        });

        base.OnModelCreating(modelBuilder);
    }
}
