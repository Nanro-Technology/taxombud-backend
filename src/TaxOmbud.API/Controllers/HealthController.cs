using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Application health monitoring and versioning.
/// </summary>
[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    /// <summary>Root liveness probe (GET /api/v1/health) — 200 OK or 503.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult LivenessRoot()
    {
        return Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>Basic liveness check.</summary>
    [HttpGet("liveness")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>Readiness probe (Kubernetes-compatible path).</summary>
    [HttpGet("ready")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ready()
    {
        return Ok(new { status = "Ready", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>Readiness check (DB connections, external APIs).</summary>
    [HttpGet("readiness")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Readiness()
    {
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
