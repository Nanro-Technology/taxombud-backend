using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Infrastructure.Options;
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

        // ─── Caching ──────────────────────────────────────────────────────────
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
        }
        else
        {
            services.AddDistributedMemoryCache(); // dev fallback
        }
        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.ICacheService, CacheService>();
        services.AddScoped<TaxOmbud.Application.Common.Interfaces.ICacheService, CacheService>();

        services.AddSingleton<TaxOmbud.Application.Interfaces.InfrastructureService.ICryptoService, CryptoService>();
        services.AddSingleton<TaxOmbud.Application.Common.Interfaces.ICryptoService, CryptoService>();

        services.AddSingleton<TaxOmbud.Application.Interfaces.InfrastructureService.IEncryptionService, EncryptionService>();
        services.AddSingleton<TaxOmbud.Application.Common.Interfaces.IEncryptionService, EncryptionService>();

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

        // ─── Application Services ─────────────────────────────────────────────
        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.ITokenService, TokenService>();
        services.AddScoped<TaxOmbud.Application.Common.Interfaces.ITokenService, TokenService>();

        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.IPasswordHasher, PasswordHasher>();
        services.AddScoped<TaxOmbud.Application.Common.Interfaces.IPasswordHasher, PasswordHasher>();

        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.IEmailService, TaxOmbud.Infrastructure.EmailServices.SmtpEmailService>();
        services.AddScoped<TaxOmbud.Application.Common.Interfaces.IEmailService, TaxOmbud.Infrastructure.EmailServices.SmtpEmailService>();

        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.IFileStorageService, LocalFileStorageService>();
        services.AddScoped<TaxOmbud.Application.Common.Interfaces.IFileStorageService, LocalFileStorageService>();

        // ─── JWT Authentication ───────────────────────────────────────────────
        // NOTE: DbContext and database config are registered in
        // TaxOmbud.Persistence (ServiceExtensions.AddPersistence).
        // Authorization policies are permission-claim based — no hardcoded role strings.
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

        // ─── Authorization (permission-claim based — no hardcoded role strings) ──
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser())
            .AddPolicy("AdminOnly", p => p.RequireRole(TaxOmbud.Domain.Entities.Identity.RoleConstants.SuperAdmin, TaxOmbud.Domain.Entities.Identity.RoleConstants.Admin))
            // Complaints
            .AddPolicy("CanViewComplaints",   p => p.RequireClaim("permission", "Complaints:View"))
            .AddPolicy("CanCreateComplaints", p => p.RequireClaim("permission", "Complaints:Create"))
            .AddPolicy("CanEditComplaints",   p => p.RequireClaim("permission", "Complaints:Edit"))
            .AddPolicy("CanDeleteComplaints", p => p.RequireClaim("permission", "Complaints:Delete"))
            // Cases
            .AddPolicy("CanViewCases",        p => p.RequireClaim("permission", "Cases:View"))
            .AddPolicy("CanCreateCases",      p => p.RequireClaim("permission", "Cases:Create"))
            .AddPolicy("CanEditCases",        p => p.RequireClaim("permission", "Cases:Edit"))
            // Users
            .AddPolicy("CanViewUsers",        p => p.RequireClaim("permission", "Users:View"))
            .AddPolicy("CanCreateUsers",      p => p.RequireClaim("permission", "Users:Create"))
            .AddPolicy("CanManageUsers",      p => p.RequireClaim("permission", "Users:Edit"))
            .AddPolicy("CanDeleteUsers",      p => p.RequireClaim("permission", "Users:Delete"))
            // Roles
            .AddPolicy("CanViewRoles",        p => p.RequireClaim("permission", "Roles:View"))
            .AddPolicy("CanManageRoles",      p => p.RequireClaim("permission", "Roles:Edit"))
            // Reports
            .AddPolicy("CanViewReports",      p => p.RequireClaim("permission", "Reports:View"))
            .AddPolicy("CanExportReports",    p => p.RequireClaim("permission", "Reports:Create"))
            // HR
            .AddPolicy("CanViewHR",           p => p.RequireClaim("permission", "HR:View"))
            .AddPolicy("CanManageHR",         p => p.RequireClaim("permission", "HR:Edit"))
            // Payroll
            .AddPolicy("CanRunPayroll",       p => p.RequireClaim("permission", "Payroll:Create"))
            .AddPolicy("CanApprovePayroll",   p => p.RequireClaim("permission", "Payroll:Edit"))
            // Finance
            .AddPolicy("CanViewFinance",      p => p.RequireClaim("permission", "Finance:View"))
            .AddPolicy("CanManageFinance",    p => p.RequireClaim("permission", "Finance:Edit"))
            // System
            .AddPolicy("CanManageSystem",     p => p.RequireClaim("permission", "System:Edit"))
            .AddPolicy("CanViewAudit",        p => p.RequireClaim("permission", "Audit:View"));

        return services;
    }
}
