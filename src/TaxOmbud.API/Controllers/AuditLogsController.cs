using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.AuditLogs.Queries.GetAuditLogById;
using TaxOmbud.Application.Features.AuditLogs.Queries.GetAuditLogs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Read-only access to the fine-grained system audit trail, including impersonation events.
/// Admin only.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/audit-logs")]
public class AuditLogsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
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
        var result = await _mediator.Send(new GetAuditLogsQuery(
            entityType,
            entityId,
            userId,
            action,
            from,
            to,
            page,
            pageSize
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>Get a specific audit log entry by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAuditLogByIdQuery(id), ct);
        return ToActionResult(result);
    }
}
