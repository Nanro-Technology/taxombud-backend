using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Departments.Commands.CreateDepartment;
using TaxOmbud.Application.Features.Departments.Commands.UpdateDepartment;
using TaxOmbud.Application.Features.Departments.Queries.GetDepartmentById;
using TaxOmbud.Application.Features.Departments.Queries.GetDepartments;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage civil service departments and their case assignment routing rules.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/departments")]
public class DepartmentsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List departments.</summary>
    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Get department by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a department.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update department details and routing configuration.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var command = new UpdateDepartmentCommand(id, request.Name, request.RoutingMode, request.Description, request.HeadUserId);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}

public record UpdateDepartmentRequest(string Name, string RoutingMode, string? Description, Guid? HeadUserId);
