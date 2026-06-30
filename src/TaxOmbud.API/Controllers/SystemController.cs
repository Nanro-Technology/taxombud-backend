using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.SystemSettings.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/system")]
public class SystemController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;

    public SystemController(ISystemSettingsService systemSettingsService)
    {
        _systemSettingsService = systemSettingsService;
    }

    /// <summary>Get current E2EE status.</summary>
    [AllowAnonymous]
    [HttpGet("settings/e2ee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetE2eeStatus(CancellationToken ct)
    {
        var result = await _systemSettingsService.GetE2eeStatusAsync(new GetE2eeStatusQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Toggle global E2EE status.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("settings/e2ee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleE2ee([FromBody] ToggleE2eeRequest request, CancellationToken ct)
    {
        var result = await _systemSettingsService.ToggleE2eeAsync(new ToggleE2eeCommand(request.Enable), ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record ToggleE2eeRequest(bool Enable);
