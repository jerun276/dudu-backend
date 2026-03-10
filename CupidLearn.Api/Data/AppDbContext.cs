using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : CupidLearn.Infrastructure.Data.AppDbContext(options)
{
}
