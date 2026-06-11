using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.PayGrades.Commands.CreatePayGrade;
using TaxOmbud.Application.Features.PayGrades.Commands.DeletePayGrade;
using TaxOmbud.Application.Features.PayGrades.Commands.SaveSalaryProfile;
using TaxOmbud.Application.Features.PayGrades.Commands.UpdatePayGrade;
using TaxOmbud.Application.Features.PayGrades.Queries.GetPayGradeById;
using TaxOmbud.Application.Features.PayGrades.Queries.GetPayGrades;
using TaxOmbud.Application.Features.PayGrades.Queries.GetSalaryProfiles;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage pay grades (salary bands/levels) used across HR payroll and staff profiling.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/hr/pay-grades")]
public class PayGradesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PayGradesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List all pay grades.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayGrades(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayGradesQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a pay grade by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayGradeById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayGradeByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a pay grade.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayGrade([FromBody] CreatePayGradeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreatePayGradeCommand(request.Name, request.Level, request.BasicSalaryBand), ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetPayGradeById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update a pay grade.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayGrade(Guid id, [FromBody] UpdatePayGradeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePayGradeCommand(id, request.Name, request.Level, request.BasicSalaryBand), ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a pay grade.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayGrade(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeletePayGradeCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>List salary profiles assigned to employees.</summary>
    [HttpGet("salary-profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryProfiles([FromQuery] Guid? userId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSalaryProfilesQuery(userId), ct);
        return ToActionResult(result);
    }

    /// <summary>Create or update a salary profile for an employee.</summary>
    [HttpPost("salary-profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSalaryProfile([FromBody] SaveSalaryProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SaveSalaryProfileCommand(
            request.UserId,
            request.Basic,
            request.Allowances,
            request.Deductions,
            request.EffectiveFrom
        ), ct);

        return ToActionResult(result);
    }
}

public record CreatePayGradeRequest(string Name, int Level, string BasicSalaryBand);
public record UpdatePayGradeRequest(string Name, int Level, string BasicSalaryBand);
public record SaveSalaryProfileRequest(
    Guid UserId,
    decimal Basic,
    string? Allowances,
    string? Deductions,
    DateTimeOffset EffectiveFrom
);
