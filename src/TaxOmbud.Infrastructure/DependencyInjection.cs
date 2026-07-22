using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Infrastructure.EmailServices;
using TaxOmbud.Infrastructure.HttpService;
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
        services.AddScoped<ICacheService, CacheService>();

        services.AddSingleton<ICryptoService, CryptoService>();

        services.AddSingleton<IEncryptionService, EncryptionService>();

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

        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddScoped<IHttpRequestManagerService, HttpRequestManagerService>();

        // ─── JWT Authentication ───────────────────────────────────────────────
        // NOTE: DbContext and database config are registered in
        // TaxOmbud.Persistence (ServiceExtensions.AddPersistence).
        // Authorization policies are permission-claim based — no hardcoded role strings.
        var jwtSection = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing.");

        var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(jwtSection.PublicKeyPem);
        var publicKey = new RsaSecurityKey(publicRsa);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, opts =>
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

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // ─── Authorization (permission-claim based — no hardcoded role strings) ──
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser())
            .AddPolicy("AdminOnly", p => p.RequireRole(RoleConstants.SuperAdmin, RoleConstants.Admin))
            .AddPolicy("OfficerOrAbove", p => p.RequireRole(RoleConstants.SuperAdmin, RoleConstants.Admin, RoleConstants.Director, RoleConstants.Manager, RoleConstants.SeniorOfficer, RoleConstants.Officer))
            .AddPolicy("TaxpayerOnly", p => p.RequireRole("Taxpayer"))
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
