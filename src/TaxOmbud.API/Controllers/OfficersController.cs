using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Officers.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage officer profile metadata, caseload capacity, and workload reporting.
/// </summary>
[ApiController]
[Route("api/v1/officers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "OfficerOrAbove")]
[Produces("application/json")]
public class OfficersController : ControllerBase
{
    private readonly IOfficersService _officersService;

    public OfficersController(IOfficersService officersService)
    {
        _officersService = officersService;
    }

    /// <summary>List all officer profiles with caseload stats.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOfficers(
        [FromQuery] Guid? departmentId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _officersService.GetOfficersAsync(new GetOfficersQuery(departmentId, search, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a specific officer profile by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetOfficerById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOfficerById(Guid id, CancellationToken ct)
    {
        var result = await _officersService.GetOfficerByIdAsync(new GetOfficerByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of officers with available capacity for case assignment.</summary>
    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] Guid? departmentId,
        [FromQuery] string? specialisation,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _officersService.GetAvailableOfficersAsync(new GetAvailableOfficersQuery(departmentId, specialisation, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create an officer profile for an existing staff user.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOfficerProfile([FromBody] CreateOfficerProfileRequest request, CancellationToken ct)
    {
        var result = await _officersService.CreateOfficerProfileAsync(new CreateOfficerProfileCommand(
            request.UserId, request.MaxCaseload, request.EmployeeNumber, request.Specialisation), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetOfficerById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update officer capacity settings.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOfficerProfile(Guid id, [FromBody] UpdateOfficerProfileRequest request, CancellationToken ct)
    {
        var result = await _officersService.UpdateOfficerProfileAsync(new UpdateOfficerProfileCommand(
            id, request.MaxCaseload, request.IsAvailable, request.EmployeeNumber, request.Specialisation), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get active caseload list for a specific officer.</summary>
    [HttpGet("{id:guid}/caseloads")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaseloads(Guid id, [FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var result = await _officersService.GetOfficerCaseloadsAsync(new GetOfficerCaseloadsQuery(id, activeOnly), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get performance metrics for a specific officer.</summary>
    [HttpGet("{id:guid}/performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOfficerPerformance(Guid id, CancellationToken ct)
    {
        var result = await _officersService.GetOfficerPerformanceAsync(new GetOfficerPerformanceQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
