using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Infrastructure.Options;
using TaxOmbud.Infrastructure.Persistence;
using TaxOmbud.Infrastructure.Services;

namespace TaxOmbud.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Options ──────────────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));

        // ─── Database ─────────────────────────────────────────────────────────
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider");
        
        if (databaseProvider == "MySql")
        {
            var connectionString = configuration.GetConnectionString("MySqlConnection");
            services.AddDbContext<ApplicationDbContext, MySqlApplicationDbContext>(options =>
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
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext, SqlServerApplicationDbContext>(options =>
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
        }

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // ─── Caching & Services ───────────────────────────────────────────────────
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
        }
        else
        {
            services.AddDistributedMemoryCache(); // dev fallback
        }
        services.AddScoped<ICacheService, CacheService>();
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // ─── Hangfire Background Jobs ─────────────────────────────────────────
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (databaseProvider == "MySql")
            {
                var mySqlConn = configuration.GetConnectionString("MySqlConnection");
                config.UseStorage(new Hangfire.MySql.MySqlStorage(mySqlConn, new Hangfire.MySql.MySqlStorageOptions
                {
                    TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "Hangfire"
                }));
            }
            else
            {
                var sqlConn = configuration.GetConnectionString("DefaultConnection");
                config.UseSqlServerStorage(sqlConn);
            }
        });

        services.AddHangfireServer();

        // ─── Application Services ─────────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ─── JWT Authentication ───────────────────────────────────────────────
        var jwtSection = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing.");

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(jwtSection.PublicKeyPem);
        var publicKey = new RsaSecurityKey(publicRsa);

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSection.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = publicKey,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser())
            .AddPolicy("AdminOnly", p => p.RequireRole("SuperAdmin", "Admin"))
            .AddPolicy("OfficerOrAbove", p => p.RequireRole("SuperAdmin", "Admin", "Manager", "Director", "SeniorOfficer", "Officer"))
            .AddPolicy("TaxpayerOnly", p => p.RequireRole("Taxpayer"));

        return services;
    }
}
