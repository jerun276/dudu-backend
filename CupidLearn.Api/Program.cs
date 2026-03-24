using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;
using CupidLearn.Api.Infrastructure;
using CupidLearn.Infrastructure;
using CupidLearn.Infrastructure.Seeding;
using CupidLearn.Api.Infrastructure.Swagger;
using CupidLearn.Infrastructure.Data;
using CupidLearn.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var envPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
    if (File.Exists(envPath))
    {
        var envValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();
            if (key.Length == 0)
            {
                continue;
            }

            var normalizedKey = key.Replace("__", ":", StringComparison.Ordinal);
            envValues[normalizedKey] = value;
        }

        builder.Configuration.AddInMemoryCollection(envValues);
    }
}

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cupid Learn API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Diagnostic Logging
    var assembly = typeof(AppDbContext).Assembly;
    logger.LogInformation("Checking migrations in assembly: {AssemblyName}", assembly.FullName);
    logger.LogInformation("Assembly Location: {Location}", assembly.Location);

    // Nuclear Diagnostic: Print all loaded assemblies starting with CupidLearn
    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName.StartsWith("CupidLearn"))
        .ToList();
    
    foreach (var a in loadedAssemblies)
    {
        logger.LogInformation("Loaded Assembly: {Name} at {Path}", a.FullName ?? "Unknown", a.Location ?? "Unknown");
    }

    // Compilable Reference Test:
    logger.LogInformation("Migration Source Check: {Status}", InitialCreate.DiagnosticId);
    
    // Nuclear Type Scan
    var allTypes = assembly.GetTypes();
    var migrationTypes = allTypes
        .Where(t => t.Name.Contains("InitialCreate") || (t.BaseType != null && t.BaseType.Name == "Migration"))
        .Select(t => t.FullName ?? t.Name)
        .ToList();

    if (migrationTypes.Any())
    {
        logger.LogInformation("DIAGNOSTIC: Found {Count} migration-related types in binary: {List}", migrationTypes.Count, string.Join(", ", migrationTypes));
        
        var contextType = typeof(AppDbContext);
        logger.LogInformation("Runtime Context Type: {FullName} from {Assembly}", contextType.FullName, contextType.Assembly.FullName);

        foreach (var tName in migrationTypes)
        {
            var typeObj = allTypes.FirstOrDefault(x => x.FullName == tName);
            if (typeObj != null)
            {
                var dbContextAttr = typeObj.GetCustomAttribute<DbContextAttribute>();
                logger.LogInformation("Migration {Name} DbContextAttribute: {TargetContext}", tName, dbContextAttr?.ContextType.FullName ?? "MISSING");
                
                if (dbContextAttr != null && dbContextAttr.ContextType != contextType)
                {
                    logger.LogWarning("MISMATCH DETECTED! Migration Context ({MigrationContext}) != Runtime Context ({RuntimeContext})", 
                        dbContextAttr.ContextType.AssemblyQualifiedName, contextType.AssemblyQualifiedName);
                }
            }
        }
    }
    else
    {
        logger.LogWarning("DIAGNOSTIC: NO MIGRATION TYPES FOUND IN BINARY BY NAME!");
    }

    try 
    {
        logger.LogInformation("Applying migrations...");
        await db.Database.MigrateAsync(CancellationToken.None);
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations");
        throw;
    }

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cupid Learn API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
