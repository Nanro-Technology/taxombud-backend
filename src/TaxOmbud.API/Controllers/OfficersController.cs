using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Officers.Commands.CreateOfficerProfile;
using TaxOmbud.Application.Features.Officers.Commands.UpdateOfficerProfile;
using TaxOmbud.Application.Features.Officers.Queries.GetAvailableOfficers;
using TaxOmbud.Application.Features.Officers.Queries.GetOfficerById;
using TaxOmbud.Application.Features.Officers.Queries.GetOfficerCaseloads;
using TaxOmbud.Application.Features.Officers.Queries.GetOfficers;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage officer profile metadata, caseload capacity, and workload reporting.
/// </summary>
[Authorize(Policy = "OfficerOrAbove")]
[Route("api/v1/officers")]
public class OfficersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OfficersController(IMediator mediator)
    {
        _mediator = mediator;
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
        var result = await _mediator.Send(new GetOfficersQuery(departmentId, search, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a specific officer profile by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOfficerById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOfficerByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create an officer profile for an existing staff user.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOfficerProfile([FromBody] CreateOfficerProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateOfficerProfileCommand(
            request.UserId,
            request.MaxCaseload,
            request.EmployeeNumber,
            request.Specialisation
        ), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetOfficerById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update officer capacity settings.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOfficerProfile(Guid id, [FromBody] UpdateOfficerProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateOfficerProfileCommand(
            id,
            request.MaxCaseload,
            request.IsAvailable,
            request.EmployeeNumber,
            request.Specialisation
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>Get active caseload list for a specific officer.</summary>
    [HttpGet("{id:guid}/caseloads")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaseloads(Guid id, [FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOfficerCaseloadsQuery(id, activeOnly), ct);
        return ToActionResult(result);
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
        var result = await _mediator.Send(new GetAvailableOfficersQuery(departmentId, specialisation, page, pageSize), ct);
        return ToActionResult(result);
    }
}

public record CreateOfficerProfileRequest(Guid UserId, int MaxCaseload, string? EmployeeNumber, string? Specialisation);
public record UpdateOfficerProfileRequest(int MaxCaseload, bool IsAvailable, string? EmployeeNumber, string? Specialisation);
