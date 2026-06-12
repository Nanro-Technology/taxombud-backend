using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Reports.Commands.ExportReport;
using TaxOmbud.Application.Features.Reports.Commands.CreateScheduledReport;
using TaxOmbud.Application.Features.Reports.Commands.DeleteScheduledReport;
using TaxOmbud.Application.Features.Reports.Commands.ToggleScheduledReport;
using TaxOmbud.Application.Features.Reports.Queries.GetAnnualReport;
using TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByRegion;
using TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByStage;
using TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByStatus;
using TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByTaxType;
using TaxOmbud.Application.Features.Reports.Queries.GetDashboard;
using TaxOmbud.Application.Features.Reports.Queries.GetMonthlyTrend;
using TaxOmbud.Application.Features.Reports.Queries.GetOfficerWorkload;
using TaxOmbud.Application.Features.Reports.Queries.GetResolutionTimeReport;
using TaxOmbud.Application.Features.Reports.Queries.GetScheduledReports;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Analytics dashboard, scheduled report configuration, and operational statistics.
/// </summary>
[Authorize(Policy = "OfficerOrAbove")]
[Route("api/v1/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Dashboard summary stats — total complaints, open cases, closed cases,
    /// appeals filed, and average resolution days.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Complaint volume breakdown by tax type.</summary>
    [HttpGet("complaints/by-tax-type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByTaxType(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintsByTaxTypeQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Complaint volume breakdown by status.</summary>
    [HttpGet("complaints/by-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByStatus(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintsByStatusQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Complaint volume breakdown by stage (queue).</summary>
    [HttpGet("complaints/by-stage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByStage(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintsByStageQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Monthly complaint volume trend for the current year.</summary>
    [HttpGet("complaints/monthly-trend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMonthlyTrendQuery(year), ct);
        return ToActionResult(result);
    }

    /// <summary>Complaint volume breakdown by region.</summary>
    [HttpGet("complaints/by-region")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplaintsByRegion(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetComplaintsByRegionQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Case resolution time metrics.</summary>
    [HttpGet("cases/resolution-time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResolutionTime([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetResolutionTimeReportQuery(year), ct);
        return ToActionResult(result);
    }

    /// <summary>Annual aggregate report.</summary>
    [HttpGet("annual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnualReport([FromQuery] int year, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAnnualReportQuery(year), ct);
        return ToActionResult(result);
    }

    /// <summary>Export report data.</summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ExportReportCommand(request.ReportType, request.Format, request.Year), ct);
        return ToActionResult(result);
    }

    /// <summary>Officer workload — active cases per officer.</summary>
    [HttpGet("officers/workload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOfficerWorkload(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOfficerWorkloadQuery(), ct);
        return ToActionResult(result);
    }

    // ─── Scheduled Reports ────────────────────────────────────────────────────

    /// <summary>List all configured scheduled reports.</summary>
    [HttpGet("scheduled")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduledReports(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetScheduledReportsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a new scheduled report.</summary>
    [HttpPost("scheduled")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateScheduledReport([FromBody] CreateScheduledReportRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateScheduledReportCommand(
            request.ReportName,
            request.CronExpression,
            request.Recipients,
            request.Format
        ), ct);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(null, result.Value);
    }

    /// <summary>Enable or disable a scheduled report.</summary>
    [HttpPut("scheduled/{id:guid}/toggle")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleScheduledReport(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleScheduledReportCommand(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Delete a scheduled report.</summary>
    [HttpDelete("scheduled/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduledReport(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteScheduledReportCommand(id), ct);
        return ToActionResult(result);
    }

    // ─── CRM & Omni Reports (Phase 15) ──────────────────────────────────────────

    [HttpGet("cases")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCaseReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetCaseReports.GetCaseReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("agents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgentReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetAgentReports.GetAgentReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetTaskReports.GetTaskReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteractionReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetInteractionReports.GetInteractionReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("sla")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlaReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetSlaReports.GetSlaReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("hr")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHrReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetHrReports.GetHrReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("erp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErpReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetErpReports.GetErpReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("time-tracking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeTrackingReports([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var result = await _mediator.Send(new TaxOmbud.Application.Features.Reports.Queries.GetTimeTrackingReports.GetTimeTrackingReportsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }
}
public record CreateScheduledReportRequest(
    string ReportName,
    string CronExpression,
    string[] Recipients,
    string? Format
);

public record ExportReportRequest(
    string ReportType,
    string Format,
    int? Year
);
