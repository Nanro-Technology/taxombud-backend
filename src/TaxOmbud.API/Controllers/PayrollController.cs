using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Payroll.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payroll")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfiles([FromQuery] GetSalaryProfilesQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetSalaryProfilesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("runs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRuns([FromQuery] GetPayrollPeriodsQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetPayrollPeriodsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("execute")]
    [Authorize(Policy = "HrOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteRun([FromBody] RunPayrollCommands command, CancellationToken ct)
    {
        var result = await _payrollService.RunPayrollAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("deductions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetDeductions(CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    [HttpGet("remittances")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRemittances([FromQuery] GetRemittancesQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetRemittancesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }
}
