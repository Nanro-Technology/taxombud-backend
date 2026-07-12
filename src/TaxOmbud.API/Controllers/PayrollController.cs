using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Payroll.DTOs;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v1/payroll")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    // ==========================================
    // 1. SALARY PROFILES
    // ==========================================
    [HttpGet("profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfiles([FromQuery] GetSalaryProfilesQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetSalaryProfilesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("profiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveProfile([FromBody] SaveSalaryProfileCommand command, CancellationToken ct)
    {
        var result = await _payrollService.SaveSalaryProfileAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("profiles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteProfile(Guid id, CancellationToken ct)
    {
        var result = await _payrollService.DeleteSalaryProfileAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 2. STATUTORY DEDUCTIONS & RULES
    // ==========================================
    [HttpGet("deductions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeductions(CancellationToken ct)
    {
        var result = await _payrollService.GetStatutoryDeductionsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("deductions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDeduction([FromBody] CreateStatutoryDeductionCommand command, CancellationToken ct)
    {
        var result = await _payrollService.CreateStatutoryDeductionAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("deductions/{id:guid}/rules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRule(Guid id, [FromBody] CreateStatutoryRuleCommand command, CancellationToken ct)
    {
        var result = await _payrollService.CreateStatutoryRuleAsync(id, command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("deductions/rules/{ruleId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRule(Guid ruleId, CancellationToken ct)
    {
        var result = await _payrollService.DeleteStatutoryRuleAsync(ruleId, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("deductions/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleDeductionStatus(Guid id, CancellationToken ct)
    {
        var result = await _payrollService.ToggleStatutoryDeductionStatusAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 3. PAYOUT PROVIDERS
    // ==========================================
    [HttpGet("providers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders(CancellationToken ct)
    {
        var result = await _payrollService.GetPayoutProvidersAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("providers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveProvider([FromBody] SavePayoutProviderCommand command, CancellationToken ct)
    {
        var result = await _payrollService.SavePayoutProviderAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("providers/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleProviderStatus(Guid id, CancellationToken ct)
    {
        var result = await _payrollService.TogglePayoutProviderStatusAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 4. PAYROLL PERIODS
    // ==========================================
    [HttpGet("periods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriods([FromQuery] GetPayrollPeriodsQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetPayrollPeriodsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("periods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePeriod([FromBody] CreatePayrollPeriodCommand command, CancellationToken ct)
    {
        var result = await _payrollService.CreatePayrollPeriodAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("periods/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TogglePeriodStatus(Guid id, CancellationToken ct)
    {
        var result = await _payrollService.TogglePayrollPeriodStatusAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("periods/{id:guid}/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePeriod(Guid id, CancellationToken ct)
    {
        var result = await _payrollService.ValidatePayrollPeriodAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 5. PAYROLL RUNS
    // ==========================================
    [HttpGet("runs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRuns(CancellationToken ct)
    {
        var result = await _payrollService.GetPayrollRunsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("runs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRun([FromBody] RunPayrollCommands command, CancellationToken ct)
    {
        var result = await _payrollService.RunPayrollAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("runs/{runId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveRun(Guid runId, CancellationToken ct)
    {
        var result = await _payrollService.ApprovePayrollAsync(new ApprovePayrollCommands(runId), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("runs/{runId:guid}/post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostRun(Guid runId, CancellationToken ct)
    {
        var result = await _payrollService.PostPayrollAsync(runId, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("runs/{runId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRun(Guid runId, CancellationToken ct)
    {
        var result = await _payrollService.DeletePayrollRunAsync(runId, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 6. SCHEDULER
    // ==========================================
    [HttpGet("scheduler")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduler(CancellationToken ct)
    {
        var result = await _payrollService.GetSchedulerConfigAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("scheduler")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveScheduler([FromBody] SchedulerConfigDto command, CancellationToken ct)
    {
        var result = await _payrollService.SaveSchedulerConfigAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("scheduler/trigger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerScheduler(CancellationToken ct)
    {
        var result = await _payrollService.TriggerSchedulerRunAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 7. REMITTANCE
    // ==========================================
    [HttpGet("remittances")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemittances([FromQuery] GetRemittancesQueries query, CancellationToken ct)
    {
        var result = await _payrollService.GetRemittancesAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("remittances")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateRemittances([FromBody] GenerateRemittanceRequest request, CancellationToken ct)
    {
        var result = await _payrollService.GenerateRemittancesAsync(request.PeriodId, request.Type, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("remittances/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRemittanceStatus(Guid id, [FromBody] UpdateRemittanceStatusRequest request, CancellationToken ct)
    {
        var result = await _payrollService.UpdateRemittanceStatusAsync(id, request.Status, ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record GenerateRemittanceRequest(Guid PeriodId, string Type);
public record UpdateRemittanceStatusRequest(string Status);
