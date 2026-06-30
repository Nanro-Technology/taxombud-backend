using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Reports.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IReportsService
{
    Task<Response<CreatedScheduledReportResponse>> CreateScheduledReportAsync(CreateScheduledReportCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteScheduledReportAsync(DeleteScheduledReportCommand request, CancellationToken cancellationToken = default);
    Task<Response<ExportReportDto>> ExportReportAsync(ExportReportCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ToggleScheduledReportAsync(ToggleScheduledReportCommand request, CancellationToken cancellationToken = default);
    Task<AgentReportDto> GetAgentReportsAsync(GetAgentReportsQuery request, CancellationToken cancellationToken = default);
    Task<Response<AnnualReportDto>> GetAnnualReportAsync(GetAnnualReportQuery request, CancellationToken cancellationToken = default);
    Task<CaseReportDto> GetCaseReportsAsync(GetCaseReportsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<RegionReportDto>>> GetComplaintsByRegionAsync(GetComplaintsByRegionQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<ComplaintsByStageDto>>> GetComplaintsByStageAsync(GetComplaintsByStageQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<ComplaintsByStatusDto>>> GetComplaintsByStatusAsync(GetComplaintsByStatusQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<ComplaintsByTaxTypeDto>>> GetComplaintsByTaxTypeAsync(GetComplaintsByTaxTypeQuery request, CancellationToken cancellationToken = default);
    Task<Response<DashboardStatsDto>> GetDashboardAsync(GetDashboardQuery request, CancellationToken cancellationToken = default);
    Task<ErpReportDto> GetErpReportsAsync(GetErpReportsQuery request, CancellationToken cancellationToken = default);
    Task<HrReportDto> GetHrReportsAsync(GetHrReportsQuery request, CancellationToken cancellationToken = default);
    Task<InteractionReportDto> GetInteractionReportsAsync(GetInteractionReportsQuery request, CancellationToken cancellationToken = default);
    Task<Response<MonthlyTrendResponseDto>> GetMonthlyTrendAsync(GetMonthlyTrendQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<OfficerWorkloadDto>>> GetOfficerWorkloadAsync(GetOfficerWorkloadQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<ResolutionTimeDto>>> GetResolutionTimeReportAsync(GetResolutionTimeReportQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<ScheduledReportDto>>> GetScheduledReportsAsync(GetScheduledReportsQuery request, CancellationToken cancellationToken = default);
    Task<SlaReportDto> GetSlaReportsAsync(GetSlaReportsQuery request, CancellationToken cancellationToken = default);
    Task<TaskReportDto> GetTaskReportsAsync(GetTaskReportsQuery request, CancellationToken cancellationToken = default);
    Task<TimeTrackingReportDto> GetTimeTrackingReportsAsync(GetTimeTrackingReportsQuery request, CancellationToken cancellationToken = default);
}
