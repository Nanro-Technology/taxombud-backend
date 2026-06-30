using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Roles.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage roles and map permissions to roles.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    /// <summary>Get list of all roles.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await _rolesService.GetRolesAsync(new GetRolesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get role by ID with permissions.</summary>
    [HttpGet("{id:guid}", Name = "GetRoleById")]
    [ProducesResponseType(typeof(Response<RoleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken ct)
    {
        var result = await _rolesService.GetRoleByIdAsync(new GetRoleByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of all individual permissions available in the system.</summary>
    [HttpGet("permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var result = await _rolesService.GetPermissionsAsync(new GetPermissionsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new role.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await _rolesService.CreateRoleAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Sync permissions to a role.</summary>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        var result = await _rolesService.UpdateRolePermissionsAsync(new UpdateRolePermissionsCommand(id, request.PermissionCodes), ct);
        return StatusCode(result.StatusCode, result);
    }
}
