using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PropertyManagement.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Ensuring database schema exists...");
        await db.Database.EnsureCreatedAsync();

        logger.LogInformation("Seeding data...");
        await DatabaseSeeder.SeedAsync(db);

        logger.LogInformation("Database ready.");
    }
}
