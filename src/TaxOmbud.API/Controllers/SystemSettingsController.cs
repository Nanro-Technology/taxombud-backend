using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.System.Commands.ImpersonateUser;
using TaxOmbud.Application.Features.System.Commands.StopImpersonation;
using TaxOmbud.Application.Features.System.Commands.ToggleFeatureFlag;
using TaxOmbud.Application.Features.System.Commands.UpdateSetting;
using TaxOmbud.Application.Features.System.Queries.GetAdminAuditLogs;
using TaxOmbud.Application.Features.System.Queries.GetFeatureFlags;
using TaxOmbud.Application.Features.System.Queries.GetSettings;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Configure feature flags, system settings, retrieve audit logs, and trigger administrative user impersonation.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/system")]
public class SystemSettingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SystemSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get list of all system settings.</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSettingsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Save or update a system setting value.</summary>
    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSettingCommand(request.Key, request.Value, request.Description), ct);
        return ToActionResult(result);
    }

    /// <summary>Get list of all application feature flags.</summary>
    [HttpGet("feature-flags")]
    public async Task<IActionResult> GetFeatureFlags(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFeatureFlagsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Toggle a feature flag's active state.</summary>
    [HttpPut("feature-flags/{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatureFlag(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleFeatureFlagCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get paginated audit logs for administration tracking.</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminAuditLogsQuery(entityName, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Start administrative impersonation of a target user.</summary>
    [HttpPost("impersonate/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImpersonateUser(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ImpersonateUserCommand(userId), ct);
        return ToActionResult(result);
    }

    /// <summary>Stop current impersonation and restore administrative identity.</summary>
    [HttpPost("impersonate/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StopImpersonation(CancellationToken ct)
    {
        var result = await _mediator.Send(new StopImpersonationCommand(), ct);
        return ToActionResult(result);
    }
}

public record UpdateSettingRequest(string Key, string Value, string? Description);
