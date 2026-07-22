using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;
using TaxOmbud.Api.Middleware;
using TaxOmbud.Api.Services;
using TaxOmbud.Application.Middlewares;
using TaxOmbud.Application;
using TaxOmbud.Infrastructure;
using TaxOmbud.Persistence.Data;
using TaxOmbud.Persistence.Extensions;

// ─── Bootstrap Logger ─────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Tax Ombud API...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/taxombud-.log",
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Information));

    // ─── Application Layers ───────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // ─── API Services ─────────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<TaxOmbud.Application.Interfaces.InfrastructureService.ICurrentUser, CurrentUserService>();
    builder.Services.AddSignalR();

    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // ─── Swagger / OpenAPI ────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Tax Ombud Case Management API",
            Version = "v1",
            Description = "RESTful API for the Tax Ombud Case Management System"
        });

        c.CustomSchemaIds(type => type.FullName);

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
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
                        Id = "Bearer"
                    }
                },
                []
            }
        });

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    });

    // ─── CORS ─────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("TaxOmbudCors", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ─── Rate Limiting ────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(opts =>
    {
        opts.AddFixedWindowLimiter("login", o =>
        {
            o.PermitLimit = 10;
            o.Window = TimeSpan.FromMinutes(1);
            o.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        });

        opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ─── Health Checks ────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    // ─── Build App ────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─── Auto-migrate & Seed on startup (Estate Management pattern) ───────────
    await app.SeedDatabaseAsync();

    // ─── Reverse-proxy path prefix (Nginx proxies /api/* → this container) ─────
    // This must come BEFORE all other middleware so routing works correctly.
    app.UsePathBase("/api");
    app.UseRouting();

    app.ConfigureCustomExceptionMiddleware();
    app.UseSerilogRequestLogging();

    // ─── Swagger (available at /api/swagger/index.html via Nginx) ────────────
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Tax Ombud API v1");
        c.RoutePrefix = "swagger";
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors("TaxOmbudCors");
    app.UseRateLimiter();
    app.UseMiddleware<E2eeMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHangfireDashboard();

    app.MapControllers();
    app.MapHub<TaxOmbud.API.Hubs.ChatHub>("/hubs/chat");
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/"); // also respond at root for Nginx upstream health checks

    Log.Information("Tax Ombud API started successfully.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Tax Ombud API terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
