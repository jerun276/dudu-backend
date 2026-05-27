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

// Unconditionally load .env if it exists (for server debugging)
var envPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
if (File.Exists(envPath))
{
    var envValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

        var equalsIndex = line.IndexOf('=');
        if (equalsIndex <= 0) continue;

        var key = line[..equalsIndex].Trim();
        var value = line[(equalsIndex + 1)..].Trim();
        if (key.Length == 0) continue;

        var normalizedKey = key.Replace("__", ":", StringComparison.Ordinal);
        envValues[normalizedKey] = value;
    }
    builder.Configuration.AddInMemoryCollection(envValues);
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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync(CancellationToken.None);
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.UseDeveloperExceptionPage(); // UNCONDITIONAL for debugging post-login crash
app.UseGlobalExceptionHandling();
app.UseCors("DevCors"); // UNCONDITIONAL for testing from browser/mobile

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


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
