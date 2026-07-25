using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.System.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Infrastructure health, API latency, database connection pool, SMTP probes, and operational monitoring. Admin only.
/// </summary>
[ApiController]
[Route("api/v1/system/monitoring")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class SystemMonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;

    public SystemMonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    /// <summary>Get real-time operational monitoring metrics snapshot.</summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var result = await _monitoringService.GetMonitoringMetricsAsync(new GetSystemMonitoringQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Run active diagnostic health probes across database, SMTP, cache, and system services.</summary>
    [HttpPost("health-check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunDiagnostics(CancellationToken ct)
    {
        var result = await _monitoringService.RunDiagnosticHealthCheckAsync(ct);
        return StatusCode(result.StatusCode, result);
    }
}
