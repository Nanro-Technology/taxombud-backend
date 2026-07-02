using Microsoft.AspNetCore.RateLimiting;
using TaxOmbud.Api.Services;

namespace TaxOmbud.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers API-layer services: CurrentUser, HttpContextAccessor, SignalR, Controllers,
    /// Swagger, CORS, RateLimiting, and HealthChecks.
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── HTTP Context & Current User ──────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.ICurrentUser, CurrentUserService>();

        // ─── Real-time ────────────────────────────────────────────────────────
        services.AddSignalR();

        // ─── Controllers ──────────────────────────────────────────────────────
        services.AddControllers(opts =>
        {
            opts.Filters.Add<TaxOmbud.Api.Filters.ValidationFilter>();
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        // ─── Swagger / OpenAPI ────────────────────────────────────────────────
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title       = "Tax Ombud Case Management API",
                Version     = "v1",
                Description = "RESTful API for the Tax Ombud Case Management System"
            });

            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme      = "Bearer",
                BearerFormat = "JWT",
                In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter your JWT access token."
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    []
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);
        });

        // ─── CORS ─────────────────────────────────────────────────────────────
        services.AddCors(options =>
        {
            options.AddPolicy("TaxOmbudCors", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // ─── Rate Limiting ────────────────────────────────────────────────────
        services.AddRateLimiter(opts =>
        {
            opts.AddFixedWindowLimiter("login", o =>
            {
                o.PermitLimit            = 10;
                o.Window                 = TimeSpan.FromMinutes(1);
                o.QueueProcessingOrder   = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                o.QueueLimit             = 0;
            });
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // ─── Health Checks ────────────────────────────────────────────────────
        services.AddHealthChecks();

        return services;
    }
}
