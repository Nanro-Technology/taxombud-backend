using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.PayGrades.DTOs;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage pay grades (salary bands/levels) used across HR payroll and staff profiling.
/// </summary>
[ApiController]
[Route("api/v1/hr/pay-grades")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class PayGradesController : ControllerBase
{
    private readonly IPayGradesService _payGradesService;

    public PayGradesController(IPayGradesService payGradesService)
    {
        _payGradesService = payGradesService;
    }

    /// <summary>List all pay grades.</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayGrades(CancellationToken ct)
    {
        var result = await _payGradesService.GetPayGradesAsync(new GetPayGradesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a pay grade by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetPayGradeById")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayGradeById(Guid id, CancellationToken ct)
    {
        var result = await _payGradesService.GetPayGradeByIdAsync(new GetPayGradeByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a pay grade.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayGrade([FromBody] CreatePayGradeRequest request, CancellationToken ct)
    {
        var result = await _payGradesService.CreatePayGradeAsync(new CreatePayGradeCommand(request.Name, request.Level, request.BasicSalaryBand), ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetPayGradeById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update a pay grade.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayGrade(Guid id, [FromBody] UpdatePayGradeRequest request, CancellationToken ct)
    {
        var result = await _payGradesService.UpdatePayGradeAsync(new UpdatePayGradeCommand(id, request.Name, request.Level, request.BasicSalaryBand), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a pay grade.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayGrade(Guid id, CancellationToken ct)
    {
        var result = await _payGradesService.DeletePayGradeAsync(new DeletePayGradeCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>List salary profiles assigned to employees.</summary>
    [HttpGet("salary-profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryProfiles([FromQuery] Guid? userId, CancellationToken ct = default)
    {
        var result = await _payGradesService.GetSalaryProfilesAsync(new GetSalaryProfilesQuery(userId), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create or update a salary profile for an employee.</summary>
    [HttpPost("salary-profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSalaryProfile([FromBody] SaveSalaryProfileRequest request, CancellationToken ct)
    {
        var result = await _payGradesService.SaveSalaryProfileAsync(new SaveSalaryProfileCommand(
            request.UserId, request.Basic, request.Allowances, request.Deductions, request.EffectiveFrom), ct);
        return StatusCode(result.StatusCode, result);
    }
}
