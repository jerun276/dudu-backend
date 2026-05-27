using Amazon.S3;
using CupidLearn.Infrastructure.Auth;
using CupidLearn.Infrastructure.Data;
using CupidLearn.Infrastructure.Email;
using CupidLearn.Infrastructure.Seeding;
using CupidLearn.Infrastructure.Services;
using CupidLearn.Infrastructure.Storage;
using CupidLearn.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CupidLearn.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Postgres");
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("CupidLearn.Infrastructure"));
        });

        services.AddJwtModule(configuration);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IProfilesService, ProfilesService>();
        services.AddScoped<IChildrenService, ChildrenService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IContentQueryService, ContentQueryService>();
        services.AddScoped<IContentAdminService, ContentAdminService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IExamAttemptService, ExamAttemptService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IOrganizationSeatService, OrganizationSeatService>();
        services.AddScoped<IAdminUsersService, AdminUsersService>();

        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));
        var r2 = configuration.GetSection(R2Options.SectionName).Get<R2Options>()!;
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            r2.AccessKeyId,
            r2.SecretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            }));
        services.AddScoped<IFileStorageService, R2StorageService>();

        return services;
    }
}
