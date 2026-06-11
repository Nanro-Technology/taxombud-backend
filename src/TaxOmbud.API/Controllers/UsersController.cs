using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Users.Commands.ApplyPermissionOverrides;
using TaxOmbud.Application.Features.Users.Commands.AssignRoles;
using TaxOmbud.Application.Features.Users.Commands.CreateUser;
using TaxOmbud.Application.Features.Users.Commands.UpdateCurrentUser;
using TaxOmbud.Application.Features.Users.Commands.UpdateUser;
using TaxOmbud.Application.Features.Users.Commands.UpdateUserStatus;
using TaxOmbud.Application.Features.Users.Queries.GetAuditLog;
using TaxOmbud.Application.Features.Users.Queries.GetCurrentUser;
using TaxOmbud.Application.Features.Users.Queries.GetUserById;
using TaxOmbud.Application.Features.Users.Queries.GetUsers;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage user accounts, role assignments, department mappings, and permission overrides.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/users")]
public class UsersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List users with optional search and filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(search, status, departmentId, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get user details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a new staff user.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update user profile details.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id, request.FirstName, request.LastName, request.Phone, request.JobTitle, request.EmploymentType, request.DepartmentId);
        
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Toggle User Status (Activate/Deactivate).</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateUserStatusCommand(id, request.Activate), ct);
        return ToActionResult(result);
    }

    /// <summary>Assign roles to user.</summary>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignRolesCommand(id, request.RoleIds), ct);
        return ToActionResult(result);
    }

    /// <summary>Override user permissions directly.</summary>
    [HttpPost("{id:guid}/permissions/overrides")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyPermissionOverrides(Guid id, [FromBody] PermissionOverridesRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApplyPermissionOverridesCommand(id, request.Overrides), ct);
        return ToActionResult(result);
    }

    /// <summary>Get the current authenticated user's profile.</summary>
    [HttpGet("me")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Update the current authenticated user's own profile.</summary>
    [HttpPatch("me")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateCurrentUserRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateCurrentUserCommand(request.FirstName, request.LastName, request.Phone, request.JobTitle), ct);
        return ToActionResult(result);
    }

    /// <summary>Get the system audit log (admin only).</summary>
    [HttpGet("audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] Guid? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAuditLogQuery(userId, entityType, action, from, to, page, pageSize), ct);
        return ToActionResult(result);
    }
}

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
);

public record UpdateUserStatusRequest(bool Activate);

public record AssignRolesRequest(Guid[] RoleIds);

public record PermissionOverridesRequest(PermissionOverrideDto[] Overrides);

public record UpdateCurrentUserRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle
);
