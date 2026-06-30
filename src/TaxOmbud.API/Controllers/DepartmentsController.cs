using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Departments.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage civil service departments and their case assignment routing rules.
/// </summary>
[ApiController]
[Route("api/v1/departments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentsService _departmentsService;

    public DepartmentsController(IDepartmentsService departmentsService)
    {
        _departmentsService = departmentsService;
    }

    /// <summary>List departments.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var result = await _departmentsService.GetDepartmentsAsync(new GetDepartmentsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get department by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetDepartmentById")]
    [ProducesResponseType(typeof(Response<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken ct)
    {
        var result = await _departmentsService.GetDepartmentByIdAsync(new GetDepartmentByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a department.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command, CancellationToken ct)
    {
        var result = await _departmentsService.CreateDepartmentAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update department details and routing configuration.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var result = await _departmentsService.UpdateDepartmentAsync(new UpdateDepartmentCommand(id, request.Name, request.RoutingMode, request.Description, request.HeadUserId), ct);
        return StatusCode(result.StatusCode, result);
    }
}
