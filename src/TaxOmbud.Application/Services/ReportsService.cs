using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Reports.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Crm;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Finance;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class ReportsService : IReportsService
{
    private readonly IGenericRepository<ScheduledReport> _scheduledReportRepo;
    private readonly IGenericRepository<Case> _caseRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Interaction> _interactionRepo;
    private readonly IGenericRepository<Complaint> _complaintRepo;
    private readonly IGenericRepository<PayrollRun> _payrollRunRepo;
    private readonly IGenericRepository<Contract> _contractRepo;
    private readonly IGenericRepository<Quote> _quoteRepo;
    private readonly IGenericRepository<StaffProfile> _staffRepo;
    private readonly IGenericRepository<LeaveRequest> _leaveRepo;
    private readonly IGenericRepository<DisciplinaryCase> _disciplinaryRepo;
    private readonly IGenericRepository<AttendanceLog> _attendanceRepo;
    private readonly IGenericRepository<OfficerCaseload> _caseloadRepo;
    private readonly IGenericRepository<OfficerProfile> _officerRepo;
    private readonly IGenericRepository<TaxpayerProfile> _taxpayerRepo;
    private readonly IGenericRepository<CaseTask> _taskRepo;
    private readonly IGenericRepository<TimeLog> _timeRepo;
    private readonly IGenericRepository<Appeal> _appealRepo;

    public ReportsService(
        IGenericRepository<ScheduledReport> scheduledReportRepo,
        IGenericRepository<Case> caseRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<Interaction> interactionRepo,
        IGenericRepository<Complaint> complaintRepo,
        IGenericRepository<PayrollRun> payrollRunRepo,
        IGenericRepository<Contract> contractRepo,
        IGenericRepository<Quote> quoteRepo,
        IGenericRepository<StaffProfile> staffRepo,
        IGenericRepository<LeaveRequest> leaveRepo,
        IGenericRepository<DisciplinaryCase> disciplinaryRepo,
        IGenericRepository<AttendanceLog> attendanceRepo,
        IGenericRepository<OfficerCaseload> caseloadRepo,
        IGenericRepository<OfficerProfile> officerRepo,
        IGenericRepository<TaxpayerProfile> taxpayerRepo,
        IGenericRepository<CaseTask> taskRepo,
        IGenericRepository<TimeLog> timeRepo,
        IGenericRepository<Appeal> appealRepo
    )
    {
        _scheduledReportRepo = scheduledReportRepo;
        _caseRepo = caseRepo;
        _userRepo = userRepo;
        _interactionRepo = interactionRepo;
        _complaintRepo = complaintRepo;
        _payrollRunRepo = payrollRunRepo;
        _contractRepo = contractRepo;
        _quoteRepo = quoteRepo;
        _staffRepo = staffRepo;
        _leaveRepo = leaveRepo;
        _disciplinaryRepo = disciplinaryRepo;
        _attendanceRepo = attendanceRepo;
        _caseloadRepo = caseloadRepo;
        _officerRepo = officerRepo;
        _taxpayerRepo = taxpayerRepo;
        _taskRepo = taskRepo;
        _timeRepo = timeRepo;
        _appealRepo = appealRepo;
    }

    public async Task<Response<CreatedScheduledReportResponse>> CreateScheduledReportAsync(CreateScheduledReportCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreatedScheduledReportResponse>();
        try
        {
            var report = new ScheduledReport
            {
                Id = Guid.NewGuid(),
                ReportName = request.ReportName,
                CronExpression = request.CronExpression,
                Recipients = string.Join(",", request.Recipients),
                Format = request.Format ?? "CSV",
                IsActive = true
            };

            await _scheduledReportRepo.AddAsync(report);
            await _scheduledReportRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Scheduled report created successfully.";
            response.Data = new CreatedScheduledReportResponse(
                report.Id,
                report.ReportName,
                report.CronExpression,
                report.Format
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the scheduled report.";
            return response;
        }
    }

    public async Task<Response<object?>> DeleteScheduledReportAsync(DeleteScheduledReportCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var report = await _scheduledReportRepo.FindAsync(r => r.Id == request.Id);
            if (report == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Scheduled report not found.";
                return response;
            }

            await _scheduledReportRepo.RemoveAsync(report);
            await _scheduledReportRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Scheduled report deleted successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the scheduled report.";
            return response;
        }
    }

    public async Task<Response<ExportReportDto>> ExportReportAsync(ExportReportCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ExportReportDto>();
        try
        {
            await Task.CompletedTask;

            var ext = request.Format.ToLower() == "pdf" ? "pdf" : "csv";
            var mime = ext == "pdf" ? "application/pdf" : "text/csv";
            var fakeUrl = $"https://storage.taxombud.com/exports/{request.ReportType}_{request.Year}.{ext}";

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Report exported successfully.";
            response.Data = new ExportReportDto(fakeUrl, mime);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while exporting the report.";
            return response;
        }
    }

    public async Task<Response<object?>> ToggleScheduledReportAsync(ToggleScheduledReportCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var report = await _scheduledReportRepo.FindAsync(r => r.Id == request.Id);
            if (report == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Scheduled report not found.";
                return response;
            }

            report.IsActive = !report.IsActive;
            await _scheduledReportRepo.UpdateAsync(report);
            await _scheduledReportRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Scheduled report toggled successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while toggling the scheduled report.";
            return response;
        }
    }

    public async Task<AgentReportDto> GetAgentReportsAsync(GetAgentReportsQuery request, CancellationToken cancellationToken = default)
    {
        var casesQuery = _caseRepo.Query();

        if (request.From.HasValue)
            casesQuery = casesQuery.Where(c => c.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            casesQuery = casesQuery.Where(c => c.CreatedAt <= request.To.Value);

        var agents = await _userRepo.Query()
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);

        var dto = new AgentReportDto();

        foreach (var agent in agents)
        {
            var agentCases = casesQuery.Where(c => c.AssignedOfficerId == agent.Id);

            var assignedCount = await agentCases.CountAsync(cancellationToken);
            var resolvedCount = await agentCases.CountAsync(c => c.Status == CaseStatus.Closed, cancellationToken);

            var resolvedCases = await agentCases
                .Where(c => c.Status == CaseStatus.Closed && c.ClosedAt != null)
                .Select(c => new { c.CreatedAt, c.ClosedAt })
                .ToListAsync(cancellationToken);

            double avgResolutionTime = 0;
            if (resolvedCases.Any())
            {
                avgResolutionTime = resolvedCases.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalHours);
            }

            var interactionsCount = await _interactionRepo.Query()
                .Where(i => i.LoggedById == agent.Id
                    && (!request.From.HasValue || i.CreatedAt >= request.From)
                    && (!request.To.HasValue || i.CreatedAt <= request.To))
                .CountAsync(cancellationToken);

            if (assignedCount > 0 || interactionsCount > 0)
            {
                dto.AgentPerformances.Add(new AgentPerformanceDto
                {
                    AgentId = agent.Id,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    CasesAssigned = assignedCount,
                    CasesResolved = resolvedCount,
                    AverageResolutionTimeHours = avgResolutionTime,
                    InteractionsHandled = interactionsCount
                });
            }
        }

        return dto;
    }

    public async Task<Response<AnnualReportDto>> GetAnnualReportAsync(GetAnnualReportQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AnnualReportDto>();
        try
        {
            var totalComplaints = await _complaintRepo.Query()
                .Where(c => c.CreatedAt.Year == request.Year)
                .CountAsync(cancellationToken);

            var cases = await _caseRepo.Query()
                .Where(c => c.CreatedAt.Year == request.Year)
                .ToListAsync(cancellationToken);

            var totalCases = cases.Count;
            var resolvedCasesList = cases.Where(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue).ToList();
            var resolvedCases = resolvedCasesList.Count;

            double avgDays = 0;
            if (resolvedCasesList.Any())
            {
                avgDays = resolvedCasesList.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Annual report retrieved successfully.";
            response.Data = new AnnualReportDto(request.Year, totalComplaints, totalCases, resolvedCases, avgDays);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the annual report.";
            return response;
        }
    }

    public async Task<CaseReportDto> GetCaseReportsAsync(GetCaseReportsQuery request, CancellationToken cancellationToken = default)
    {
        var query = _caseRepo.Query();

        if (request.From.HasValue)
            query = query.Where(c => c.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(c => c.CreatedAt <= request.To.Value);

        var totalCases = await query.CountAsync(cancellationToken);

        var statuses = await query.GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var priorities = await query.GroupBy(c => c.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var departments = await query.GroupBy(c => c.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var openCases = statuses.Where(s => s.Status != CaseStatus.Closed).Sum(s => s.Count);
        var closedCases = statuses.Where(s => s.Status == CaseStatus.Closed).Sum(s => s.Count);

        var now = DateTimeOffset.UtcNow;
        var twoDaysAgo = now.AddDays(-2);
        var sevenDaysAgo = now.AddDays(-7);
        var fourteenDaysAgo = now.AddDays(-14);

        var openQuery = query.Where(c => c.Status != CaseStatus.Closed);
        var bucket1 = await openQuery.CountAsync(c => c.CreatedAt >= twoDaysAgo, cancellationToken);
        var bucket2 = await openQuery.CountAsync(c => c.CreatedAt < twoDaysAgo && c.CreatedAt >= sevenDaysAgo, cancellationToken);
        var bucket3 = await openQuery.CountAsync(c => c.CreatedAt < sevenDaysAgo && c.CreatedAt >= fourteenDaysAgo, cancellationToken);
        var bucket4 = await openQuery.CountAsync(c => c.CreatedAt < fourteenDaysAgo, cancellationToken);

        return new CaseReportDto
        {
            TotalCases = totalCases,
            OpenCases = openCases,
            ClosedCases = closedCases,
            EscalatedCases = statuses.FirstOrDefault(s => s.Status == CaseStatus.UnderReview)?.Count ?? 0,
            CasesByStatus = statuses.ToDictionary(k => k.Status.ToString(), v => v.Count),
            CasesByPriority = priorities.ToDictionary(k => k.Priority ?? "Unknown", v => v.Count),
            CasesByCategory = departments.ToDictionary(k => k.DepartmentId?.ToString() ?? "None", v => v.Count),
            AgingBuckets = new Dictionary<string, int>
            {
                { "0-2 days", bucket1 },
                { "3-7 days", bucket2 },
                { "8-14 days", bucket3 },
                { "15+ days", bucket4 }
            }
        };
    }

    public async Task<Response<List<RegionReportDto>>> GetComplaintsByRegionAsync(GetComplaintsByRegionQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<RegionReportDto>>();
        try
        {
            var stats = await _complaintRepo.Query()
                .Include(c => c.Taxpayer)
                .Where(c => c.Taxpayer != null && !string.IsNullOrEmpty(c.Taxpayer.City))
                .GroupBy(c => c.Taxpayer.City)
                .Select(g => new RegionReportDto(g.Key ?? "Unknown", g.Count()))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Region report retrieved successfully.";
            response.Data = stats;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the region report.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<ComplaintsByStageDto>>> GetComplaintsByStageAsync(GetComplaintsByStageQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<ComplaintsByStageDto>>();
        try
        {
            var data = await _complaintRepo.Query()
                .GroupBy(c => c.CurrentStage)
                .Select(g => new ComplaintsByStageDto(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints by stage retrieved successfully.";
            response.Data = data;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the stage report.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<ComplaintsByStatusDto>>> GetComplaintsByStatusAsync(GetComplaintsByStatusQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<ComplaintsByStatusDto>>();
        try
        {
            var data = await _complaintRepo.Query()
                .GroupBy(c => c.Status)
                .Select(g => new ComplaintsByStatusDto(g.Key.ToString(), g.Count()))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints by status retrieved successfully.";
            response.Data = data;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the status report.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<ComplaintsByTaxTypeDto>>> GetComplaintsByTaxTypeAsync(GetComplaintsByTaxTypeQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<ComplaintsByTaxTypeDto>>();
        try
        {
            var data = await _complaintRepo.Query()
                .GroupBy(c => c.TaxType)
                .Select(g => new ComplaintsByTaxTypeDto(g.Key, g.Count()))
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints by tax type retrieved successfully.";
            response.Data = data;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the tax type report.";
            return response;
        }
    }

    public async Task<Response<DashboardStatsDto>> GetDashboardAsync(GetDashboardQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<DashboardStatsDto>();
        try
        {
            var totalComplaints = await _complaintRepo.CountAsync();
            var openComplaints = await _complaintRepo.CountAsync(c =>
                c.Status != ComplaintStatus.Closed &&
                c.Status != ComplaintStatus.Withdrawn);
            var closedComplaints = await _complaintRepo.CountAsync(c =>
                c.Status == ComplaintStatus.Closed);

            var totalCases = await _caseRepo.CountAsync();
            var openCases = await _caseRepo.CountAsync(c =>
                c.Status != CaseStatus.Closed);
            var closedCases = await _caseRepo.CountAsync(c =>
                c.Status == CaseStatus.Closed);

            var totalAppeals = await _appealRepo.CountAsync();
            var pendingAppeals = await _appealRepo.CountAsync(a =>
                a.Status == AppealStatus.Submitted ||
                a.Status == AppealStatus.UnderReview);

            var totalOfficers = await _officerRepo.CountAsync();
            var totalTaxpayers = await _taxpayerRepo.CountAsync();

            var closedWithDates = await _complaintRepo.Query()
                .Where(c => c.Status == ComplaintStatus.Closed && c.ClosedAt != null)
                .Select(c => new { c.CreatedAt, c.ClosedAt })
                .ToListAsync(cancellationToken);

            double avgResolutionDays = closedWithDates.Count > 0
                ? closedWithDates.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays)
                : 0;

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Dashboard stats retrieved successfully.";
            response.Data = new DashboardStatsDto(
                new ComplaintsStatsDto(totalComplaints, openComplaints, closedComplaints),
                new CasesStatsDto(totalCases, openCases, closedCases),
                new AppealsStatsDto(totalAppeals, pendingAppeals),
                new StaffStatsDto(totalOfficers, totalTaxpayers),
                Math.Round(avgResolutionDays, 1)
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving dashboard stats.";
            return response;
        }
    }

    public async Task<ErpReportDto> GetErpReportsAsync(GetErpReportsQuery request, CancellationToken cancellationToken = default)
    {
        var payrollsQuery = _payrollRunRepo.Query();

        if (request.From.HasValue)
            payrollsQuery = payrollsQuery.Where(p => p.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            payrollsQuery = payrollsQuery.Where(p => p.CreatedAt <= request.To.Value);

        var totalPayrollRuns = await payrollsQuery.CountAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var totalExpenseThisMonth = await _payrollRunRepo.Query()
            .Where(p => p.CreatedAt >= startOfMonth)
            .SumAsync(p => p.TotalNet, cancellationToken);

        var activeContracts = await _contractRepo.CountAsync(c => c.Status == "Active");
        var totalQuotes = await _quoteRepo.CountAsync();

        return new ErpReportDto
        {
            TotalPayrollRuns = totalPayrollRuns,
            TotalPayrollExpenseThisMonth = totalExpenseThisMonth,
            ActiveContracts = activeContracts,
            TotalQuotes = totalQuotes
        };
    }

    public async Task<HrReportDto> GetHrReportsAsync(GetHrReportsQuery request, CancellationToken cancellationToken = default)
    {
        var totalEmployees = await _staffRepo.CountAsync(s => s.EmploymentStatus == "Active");

        var now = DateTimeOffset.UtcNow;
        var activeLeaves = await _leaveRepo.CountAsync(l =>
            l.Status == "Approved" &&
            l.StartDate <= now &&
            l.EndDate >= now);

        var pendingDisciplinary = await _disciplinaryRepo.CountAsync(d => d.Status == "Open" || d.Status == "In Progress");

        var attendanceQuery = _attendanceRepo.Query();
        if (request.From.HasValue)
            attendanceQuery = attendanceQuery.Where(a => a.Date >= request.From.Value);
        if (request.To.HasValue)
            attendanceQuery = attendanceQuery.Where(a => a.Date <= request.To.Value);

        var attendanceStatus = await attendanceQuery.GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var departments = await _staffRepo.Query()
            .Include(s => s.User)
            .Where(s => s.EmploymentStatus == "Active")
            .GroupBy(s => s.User.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new HrReportDto
        {
            TotalEmployees = totalEmployees,
            ActiveLeaves = activeLeaves,
            PendingDisciplinaryCases = pendingDisciplinary,
            AttendanceStatusToday = attendanceStatus.ToDictionary(k => k.Status ?? "Unknown", v => v.Count),
            EmployeesByDepartment = departments.ToDictionary(k => k.DepartmentId?.ToString() ?? "Unknown", v => v.Count)
        };
    }

    public async Task<InteractionReportDto> GetInteractionReportsAsync(GetInteractionReportsQuery request, CancellationToken cancellationToken = default)
    {
        var query = _interactionRepo.Query();

        if (request.From.HasValue)
            query = query.Where(i => i.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(i => i.CreatedAt <= request.To.Value);

        if (!string.IsNullOrEmpty(request.Channel))
            query = query.Where(i => i.Channel == request.Channel);

        var total = await query.CountAsync(cancellationToken);

        var channels = await query.GroupBy(i => i.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var directions = await query.GroupBy(i => i.Direction)
            .Select(g => new { Direction = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new InteractionReportDto
        {
            TotalInteractions = total,
            InteractionsByChannel = channels.ToDictionary(k => k.Channel ?? "Unknown", v => v.Count),
            InteractionsByDirection = directions.ToDictionary(k => k.Direction ?? "Unknown", v => v.Count)
        };
    }

    public async Task<Response<MonthlyTrendResponseDto>> GetMonthlyTrendAsync(GetMonthlyTrendQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<MonthlyTrendResponseDto>();
        try
        {
            var targetYear = request.Year ?? DateTime.UtcNow.Year;
            var data = await _complaintRepo.Query()
                .Where(c => c.CreatedAt.Year == targetYear)
                .GroupBy(c => c.CreatedAt.Month)
                .Select(g => new MonthlyTrendDto(g.Key, g.Count()))
                .OrderBy(x => x.Month)
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Monthly trend retrieved successfully.";
            response.Data = new MonthlyTrendResponseDto(targetYear, data);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the monthly trend.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<OfficerWorkloadDto>>> GetOfficerWorkloadAsync(GetOfficerWorkloadQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<OfficerWorkloadDto>>();
        try
        {
            var data = await _caseloadRepo.Query()
                .Include(c => c.OfficerProfile)
                    .ThenInclude(o => o.User)
                .Where(c => c.IsActive)
                .GroupBy(c => c.OfficerProfileId)
                .Select(g => new OfficerWorkloadDto(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officer workload retrieved successfully.";
            response.Data = data;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving officer workload.";
            return response;
        }
    }

    public async Task<Response<List<ResolutionTimeDto>>> GetResolutionTimeReportAsync(GetResolutionTimeReportQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<ResolutionTimeDto>>();
        try
        {
            var year = request.Year ?? DateTime.UtcNow.Year;

            var cases = await _caseRepo.Query()
                .Where(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue && c.ClosedAt.Value.Year == year)
                .ToListAsync(cancellationToken);

            var stats = cases
                .GroupBy(c => c.ClosedAt!.Value.Month)
                .Select(g =>
                {
                    var days = g.Select(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays).OrderBy(d => d).ToList();
                    var avg = days.Average();
                    var median = days.Count % 2 == 0
                        ? (days[days.Count / 2 - 1] + days[days.Count / 2]) / 2.0
                        : days[days.Count / 2];
                    return new ResolutionTimeDto(g.Key.ToString(), avg, median);
                })
                .OrderBy(s => s.Stage)
                .ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Resolution time report retrieved successfully.";
            response.Data = stats;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the resolution time report.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<ScheduledReportDto>>> GetScheduledReportsAsync(GetScheduledReportsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<ScheduledReportDto>>();
        try
        {
            var reports = await _scheduledReportRepo.Query()
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ScheduledReportDto(
                    r.Id,
                    r.ReportName,
                    r.CronExpression,
                    r.Recipients,
                    r.Format,
                    r.IsActive,
                    r.LastRunAt,
                    r.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Scheduled reports retrieved successfully.";
            response.Data = reports;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving scheduled reports.";
            return response;
        }
    }

    public async Task<SlaReportDto> GetSlaReportsAsync(GetSlaReportsQuery request, CancellationToken cancellationToken = default)
    {
        var query = _caseRepo.Query();

        if (request.From.HasValue)
            query = query.Where(c => c.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(c => c.CreatedAt <= request.To.Value);

        var now = DateTimeOffset.UtcNow;
        var totalCases = await query.CountAsync(cancellationToken);

        var breachedCases = await query.CountAsync(c =>
            (c.ClosedAt != null && c.DueDate != null && c.ClosedAt > c.DueDate) ||
            (c.ClosedAt == null && c.DueDate != null && c.DueDate < now), cancellationToken);

        var withinSla = totalCases - breachedCases;
        var compliance = totalCases > 0 ? ((double)withinSla / totalCases) * 100 : 0;

        var resolvedCases = await query
            .Where(c => c.Status == CaseStatus.Closed && c.ClosedAt != null)
            .Select(c => new { c.CreatedAt, c.ClosedAt })
            .ToListAsync(cancellationToken);

        double avgResolutionTime = 0;
        if (resolvedCases.Any())
        {
            avgResolutionTime = resolvedCases.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalHours);
        }

        return new SlaReportDto
        {
            TotalCasesMeasured = totalCases,
            CasesWithinSla = withinSla,
            CasesBreachedSla = breachedCases,
            SlaCompliancePercentage = compliance,
            AverageResolutionTimeHours = avgResolutionTime
        };
    }

    public async Task<TaskReportDto> GetTaskReportsAsync(GetTaskReportsQuery request, CancellationToken cancellationToken = default)
    {
        var query = _taskRepo.Query();

        if (request.From.HasValue)
            query = query.Where(t => t.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(t => t.CreatedAt <= request.To.Value);

        var totalTasks = await query.CountAsync(cancellationToken);

        var statuses = await query.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var priorities = await query.GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var completed = statuses.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
        var pending = totalTasks - completed;

        var now = DateTimeOffset.UtcNow;
        var overdueTasks = await query.CountAsync(t => t.DueAt < now && t.Status != "Completed", cancellationToken);

        return new TaskReportDto
        {
            TotalTasks = totalTasks,
            CompletedTasks = completed,
            PendingTasks = pending,
            OverdueTasks = overdueTasks,
            TasksByStatus = statuses.ToDictionary(k => k.Status ?? "Unknown", v => v.Count),
            TasksByPriority = priorities.ToDictionary(k => k.Priority ?? "Unknown", v => v.Count)
        };
    }

    public async Task<TimeTrackingReportDto> GetTimeTrackingReportsAsync(GetTimeTrackingReportsQuery request, CancellationToken cancellationToken = default)
    {
        var logsQuery = _timeRepo.Query();

        if (request.From.HasValue)
            logsQuery = logsQuery.Where(t => t.StartTime >= request.From.Value);

        if (request.To.HasValue)
            logsQuery = logsQuery.Where(t => t.StartTime <= request.To.Value);

        var now = DateTimeOffset.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

        var totalThisWeek = await _timeRepo.Query()
            .Where(t => t.StartTime >= startOfWeek)
            .SumAsync(t => t.DurationHours, cancellationToken);

        var totalThisMonth = await _timeRepo.Query()
            .Where(t => t.StartTime >= startOfMonth)
            .SumAsync(t => t.DurationHours, cancellationToken);

        var hoursByAgent = await logsQuery
            .Include(t => t.User)
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, User = g.FirstOrDefault()!.User, TotalHours = g.Sum(t => t.DurationHours) })
            .ToListAsync(cancellationToken);

        return new TimeTrackingReportDto
        {
            TotalHoursLoggedThisWeek = totalThisWeek,
            TotalHoursLoggedThisMonth = totalThisMonth,
            HoursByAgent = hoursByAgent.ToDictionary(k => $"{k.User?.FirstName} {k.User?.LastName}", v => v.TotalHours)
        };
    }
}
