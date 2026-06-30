using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.System.DTOs;

namespace TaxOmbud.Api.Controllers;

public record UpdateSettingRequest(string Key, string Value, string? Description);

/// <summary>
/// Configure feature flags, system settings, retrieve audit logs, and trigger administrative user impersonation.
/// </summary>
[ApiController]
[Route("api/v1/system")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemService _systemService;

    public SystemSettingsController(ISystemService systemService)
    {
        _systemService = systemService;
    }

    /// <summary>Get list of all system settings.</summary>
    [HttpGet("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _systemService.GetSettingsAsync(new GetSettingsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Save or update a system setting value.</summary>
    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        var result = await _systemService.UpdateSettingAsync(new UpdateSettingCommand(request.Key, request.Value, request.Description), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of all application feature flags.</summary>
    [HttpGet("feature-flags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatureFlags(CancellationToken ct)
    {
        var result = await _systemService.GetFeatureFlagsAsync(new GetFeatureFlagsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Toggle a feature flag's active state.</summary>
    [HttpPut("feature-flags/{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatureFlag(Guid id, CancellationToken ct)
    {
        var result = await _systemService.ToggleFeatureFlagAsync(new ToggleFeatureFlagCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get paginated audit logs for administration tracking.</summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _systemService.GetAdminAuditLogsAsync(new GetAdminAuditLogsQuery(entityName, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Start administrative impersonation of a target user.</summary>
    [HttpPost("impersonate/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImpersonateUser(Guid userId, CancellationToken ct)
    {
        var result = await _systemService.ImpersonateUserAsync(new ImpersonateUserCommand(userId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Stop current impersonation and restore administrative identity.</summary>
    [HttpPost("impersonate/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StopImpersonation(CancellationToken ct)
    {
        var result = await _systemService.StopImpersonationAsync(new StopImpersonationCommand(), ct);
        return StatusCode(result.StatusCode, result);
    }
}
