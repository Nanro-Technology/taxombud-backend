using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Common.Config;
using TaxOmbud.Persistence.Data;
using TaxOmbud.Persistence.Repositories;

namespace TaxOmbud.Persistence.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers the database context, repositories, unit-of-work, and EF configurations.
    /// Migrations live in this (Persistence) assembly. Seeding is handled by DataSeeder.
    /// Infrastructure registers external services (cache, Hangfire, email, etc.).
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Options ──────────────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

        // ─── Database ─────────────────────────────────────────────────────────
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider");

        if (databaseProvider == "MySql")
        {
            var connectionString = configuration.GetConnectionString("MySqlConnection");
            services.AddDbContext<MySqlApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.Parse("8.0.32-mysql"),
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        sql.CommandTimeout(60);
                    });
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });
            services.AddScoped<ApplicationDbContext>(provider =>
                provider.GetRequiredService<MySqlApplicationDbContext>());
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SqlServerApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString,
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        sql.CommandTimeout(60);
                    });
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });
            services.AddScoped<ApplicationDbContext>(provider =>
                provider.GetRequiredService<SqlServerApplicationDbContext>());
        }

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<TaxOmbud.Application.Common.Interfaces.IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // ─── Repositories ─────────────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
