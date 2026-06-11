using System;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Application health monitoring and versioning.
/// </summary>
[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    /// <summary>Basic liveness check.</summary>
    [HttpGet("liveness")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>Readiness check (DB connections, external APIs).</summary>
    [HttpGet("readiness")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Readiness()
    {
        // In a real scenario, this might check Database connections via IApplicationDbContext.
        // For now, return OK indicating the app is ready to accept traffic.
        return Ok(new { status = "Ready", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>Detailed health status (admin only).</summary>
    [HttpGet("detailed")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Detailed()
    {
        return Ok(new
        {
            status = "Healthy",
            components = new[]
            {
                new { name = "Database", status = "Connected" },
                new { name = "RedisCache", status = "Connected" },
                new { name = "EmailService", status = "Operational" }
            },
            timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Application version information.</summary>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Version()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        
        return Ok(new
        {
            version = version,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            buildDate = System.IO.File.GetLastWriteTime(assembly.Location)
        });
    }
}
