using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Persistence.Data;

namespace TaxOmbud.Persistence.Extensions;

/// <summary>
/// Extension to apply pending migrations and seed reference data on startup.
/// Call app.SeedDatabaseAsync() in Program.cs after building the app.
/// </summary>
public static class DatabaseSeedingExtensions
{
    public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context     = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var logger      = services.GetRequiredService<ILogger<DataSeeder>>();

            var dbProvider = context.Database.ProviderName;
            if (dbProvider != null && dbProvider.Contains("MySql"))
            {
                logger.LogInformation("Ensuring MySQL database is created...");
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                logger.LogInformation("Applying pending EF migrations...");
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Starting database seeding...");
            var seeder = new DataSeeder(context, userManager, logger);
            await seeder.SeedAllAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DataSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw; // Re-throw to prevent app from starting with incomplete seed
        }

        return app;
    }
}
