using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Reports.DTOs;

namespace TaxOmbud.Api.Controllers;

public record CreateScheduledReportRequest(string ReportName, string CronExpression, string[] Recipients, string? Format);
public record ExportReportRequest(string ReportType, string Format, int? Year);

/// <summary>
/// Analytics dashboard, scheduled report configuration, and operational statistics.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = "OfficerOrAbove")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    /// <summary>
    /// Dashboard summary stats — total complaints, open cases, closed cases,
    /// appeals filed, and average resolution days.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _reportsService.GetDashboardAsync(new GetDashboardQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Complaint volume breakdown by tax type.</summary>
    [HttpGet("complaints/by-tax-type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByTaxType(CancellationToken ct)
    {
        var result = await _reportsService.GetComplaintsByTaxTypeAsync(new GetComplaintsByTaxTypeQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Complaint volume breakdown by status.</summary>
    [HttpGet("complaints/by-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByStatus(CancellationToken ct)
    {
        var result = await _reportsService.GetComplaintsByStatusAsync(new GetComplaintsByStatusQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Complaint volume breakdown by stage (queue).</summary>
    [HttpGet("complaints/by-stage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByStage(CancellationToken ct)
    {
        var result = await _reportsService.GetComplaintsByStageAsync(new GetComplaintsByStageQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Monthly complaint volume trend for the current year.</summary>
    [HttpGet("complaints/monthly-trend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _reportsService.GetMonthlyTrendAsync(new GetMonthlyTrendQuery(year), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Complaint volume breakdown by region.</summary>
    [HttpGet("complaints/by-region")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByRegion(CancellationToken ct)
    {
        var result = await _reportsService.GetComplaintsByRegionAsync(new GetComplaintsByRegionQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Case resolution time metrics.</summary>
    [HttpGet("cases/resolution-time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResolutionTime([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _reportsService.GetResolutionTimeReportAsync(new GetResolutionTimeReportQuery(year), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Officer workload — active cases per officer.</summary>
    [HttpGet("officers/workload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOfficerWorkload(CancellationToken ct)
    {
        var result = await _reportsService.GetOfficerWorkloadAsync(new GetOfficerWorkloadQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Annual aggregate report.</summary>
    [HttpGet("annual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnualReport([FromQuery] int year, CancellationToken ct)
    {
        var result = await _reportsService.GetAnnualReportAsync(new GetAnnualReportQuery(year), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Export report data.</summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request, CancellationToken ct)
    {
        var result = await _reportsService.ExportReportAsync(new ExportReportCommand(request.ReportType, request.Format, request.Year), ct);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Scheduled Reports ─────────────────────────────────────────────────────

    /// <summary>List all configured scheduled reports.</summary>
    [HttpGet("scheduled")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduledReports(CancellationToken ct)
    {
        var result = await _reportsService.GetScheduledReportsAsync(new GetScheduledReportsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new scheduled report.</summary>
    [HttpPost("scheduled")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScheduledReport([FromBody] CreateScheduledReportRequest request, CancellationToken ct)
    {
        var result = await _reportsService.CreateScheduledReportAsync(new CreateScheduledReportCommand(
            request.ReportName, request.CronExpression, request.Recipients, request.Format), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Enable or disable a scheduled report.</summary>
    [HttpPut("scheduled/{id:guid}/toggle")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleScheduledReport(Guid id, CancellationToken ct)
    {
        var result = await _reportsService.ToggleScheduledReportAsync(new ToggleScheduledReportCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a scheduled report.</summary>
    [HttpDelete("scheduled/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduledReport(Guid id, CancellationToken ct)
    {
        var result = await _reportsService.DeleteScheduledReportAsync(new DeleteScheduledReportCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
