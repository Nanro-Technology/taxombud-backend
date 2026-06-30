using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Users.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage user accounts, role assignments, department mappings, and permission overrides.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    /// <summary>List users with optional search and filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Response<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _usersService.GetUsersAsync(new GetUsersQuery(search, status, departmentId, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get user details by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(typeof(Response<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await _usersService.GetUserByIdAsync(new GetUserByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get the current authenticated user's profile.</summary>
    [HttpGet("me")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Response<UserDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _usersService.GetCurrentUserAsync(new GetCurrentUserQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new staff user.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _usersService.CreateUserAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update user profile details.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id, request.FirstName, request.LastName, request.Phone, request.JobTitle, request.EmploymentType, request.DepartmentId);
        var result = await _usersService.UpdateUserAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Toggle User Status (Activate/Deactivate).</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var result = await _usersService.UpdateUserStatusAsync(new UpdateUserStatusCommand(id, request.Activate), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Assign roles to user.</summary>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken ct)
    {
        var result = await _usersService.AssignRolesAsync(new AssignRolesCommand(id, request.RoleIds), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Override user permissions directly.</summary>
    [HttpPost("{id:guid}/permissions/overrides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyPermissionOverrides(Guid id, [FromBody] PermissionOverridesRequest request, CancellationToken ct)
    {
        var result = await _usersService.ApplyPermissionOverridesAsync(new ApplyPermissionOverridesCommand(id, request.Overrides), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update the current authenticated user's own profile.</summary>
    [HttpPatch("me")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateCurrentUserRequest request, CancellationToken ct)
    {
        var result = await _usersService.UpdateCurrentUserAsync(new UpdateCurrentUserCommand(request.FirstName, request.LastName, request.Phone, request.JobTitle), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get the audit log for a specific user (admin only).</summary>
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog(
        Guid id,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _usersService.GetAuditLogAsync(new GetAuditLogQuery(id, entityType, action, from, to, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }
}
