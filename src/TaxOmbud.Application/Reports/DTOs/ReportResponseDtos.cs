using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Reports.DTOs;

public class ReportFilterDto
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public class AgentReportDto
{
    public List<AgentPerformanceDto> AgentPerformances { get; set; } = new();
}

public class AgentPerformanceDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int CasesAssigned { get; set; }
    public int CasesResolved { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public int InteractionsHandled { get; set; }
}

public record AnnualReportDto(
    int Year,
    int TotalComplaints,
    int TotalCases,
    int ResolvedCases,
    double AvgResolutionDays
);

public class CaseReportDto
{
    public int TotalCases { get; set; }
    public int OpenCases { get; set; }
    public int ClosedCases { get; set; }
    public int EscalatedCases { get; set; }
    public Dictionary<string, int> CasesByStatus { get; set; } = new();
    public Dictionary<string, int> CasesByPriority { get; set; } = new();
    public Dictionary<string, int> CasesByCategory { get; set; } = new();
    public Dictionary<string, int> AgingBuckets { get; set; } = new();
}

public record RegionReportDto(
    string Region,
    int TotalComplaints
);

public class ErpReportDto
{
    public int TotalPayrollRuns { get; set; }
    public decimal TotalPayrollExpenseThisMonth { get; set; }
    public int ActiveContracts { get; set; }
    public int TotalQuotes { get; set; }
}

public class HrReportDto
{
    public int TotalEmployees { get; set; }
    public int ActiveLeaves { get; set; }
    public int PendingDisciplinaryCases { get; set; }
    public Dictionary<string, int> AttendanceStatusToday { get; set; } = new();
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
}

public class InteractionReportDto
{
    public int TotalInteractions { get; set; }
    public Dictionary<string, int> InteractionsByChannel { get; set; } = new();
    public Dictionary<string, int> InteractionsByDirection { get; set; } = new();
}

public class SlaReportDto
{
    public int TotalCasesMeasured { get; set; }
    public int CasesWithinSla { get; set; }
    public int CasesBreachedSla { get; set; }
    public double SlaCompliancePercentage { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class TaskReportDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
}

public class TimeTrackingReportDto
{
    public double TotalHoursLoggedThisWeek { get; set; }
    public double TotalHoursLoggedThisMonth { get; set; }
    public Dictionary<string, double> HoursByAgent { get; set; } = new();
}

public record ResolutionTimeDto(
    string Stage,
    double AverageDays,
    double MedianDays
);
