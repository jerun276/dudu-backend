using CupidLearn.Infrastructure.Auth;
using CupidLearn.Infrastructure.Data;
using CupidLearn.Infrastructure.Seeding;
using CupidLearn.Infrastructure.Services;
using CupidLearn.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CupidLearn.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Postgres");
            options.UseNpgsql(connectionString);
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

        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
