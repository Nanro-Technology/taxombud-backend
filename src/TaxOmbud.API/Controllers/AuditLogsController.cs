using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.AuditLogs.DTOs;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Read-only access to the fine-grained system audit trail, including impersonation events. Admin only.
/// </summary>
[ApiController]
[Route("api/v1/audit-logs")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogsService _auditLogsService;

    public AuditLogsController(IAuditLogsService auditLogsService)
    {
        _auditLogsService = auditLogsService;
    }

    /// <summary>Query audit logs with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _auditLogsService.GetAuditLogsAsync(new GetAuditLogsQuery(entityType, entityId, userId, action, from, to, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a specific audit log entry by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken ct)
    {
        var result = await _auditLogsService.GetAuditLogByIdAsync(new GetAuditLogByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
