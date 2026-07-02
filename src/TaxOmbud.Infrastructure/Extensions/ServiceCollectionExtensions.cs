using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Config;
using TaxOmbud.Infrastructure.HangfireServices;
using TaxOmbud.Infrastructure.Services;

namespace TaxOmbud.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Options ──────────────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));

        // ─── Caching ──────────────────────────────────────────────────────────
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
        else
            services.AddDistributedMemoryCache();

        services.AddScoped<ICacheService, CacheService>();

        // ─── Crypto / Security ────────────────────────────────────────────────
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // ─── Token & Password ─────────────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // ─── Email ────────────────────────────────────────────────────────────
        services.AddScoped<IEmailService, TaxOmbud.Infrastructure.EmailServices.SmtpEmailService>();

        // ─── File Storage ─────────────────────────────────────────────────────
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ─── Hangfire Background Jobs ─────────────────────────────────────────
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider");

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
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // ─── JWT Authentication ───────────────────────────────────────────────
        var jwtSection = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing.");

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(jwtSection.PublicKeyPem);
        var publicKey = new RsaSecurityKey(publicRsa);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer      = true,
                    ValidIssuer         = jwtSection.Issuer,
                    ValidateAudience    = true,
                    ValidAudience       = jwtSection.Audience,
                    ValidateLifetime    = true,
                    IssuerSigningKey    = publicKey,
                    ValidAlgorithms     = [SecurityAlgorithms.RsaSha256],
                    ClockSkew           = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser())
            .AddPolicy("AdminOnly",            p => p.RequireRole("SuperAdmin", "Admin"))
            .AddPolicy("OfficerOrAbove",       p => p.RequireRole("SuperAdmin", "Admin", "Manager", "Director", "SeniorOfficer", "Officer"))
            .AddPolicy("TaxpayerOnly",         p => p.RequireRole("Taxpayer"));

        return services;
    }
}
