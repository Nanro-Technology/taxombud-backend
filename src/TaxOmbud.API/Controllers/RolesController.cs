using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Roles.Commands.CreateRole;
using TaxOmbud.Application.Features.Roles.Commands.UpdateRolePermissions;
using TaxOmbud.Application.Features.Roles.Queries.GetPermissions;
using TaxOmbud.Application.Features.Roles.Queries.GetRoleById;
using TaxOmbud.Application.Features.Roles.Queries.GetRoles;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage roles and map permissions to roles.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/roles")]
public class RolesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get list of all roles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRolesQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Get role by ID with permissions.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a new role.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetRoleById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Sync permissions to a role.</summary>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateRolePermissionsCommand(id, request.PermissionCodes), ct);
        return ToActionResult(result);
    }

    /// <summary>Get list of all individual permissions available in the system.</summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPermissionsQuery(), ct);
        return ToActionResult(result);
    }
}

public record UpdateRolePermissionsRequest(string[] PermissionCodes);
