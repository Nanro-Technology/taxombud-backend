using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Reports.DTOs;

public class HrReportDto
{
    public int TotalEmployees { get; set; }
    public int ActiveLeaves { get; set; }
    public int PendingDisciplinaryCases { get; set; }

    public Dictionary<string, int> AttendanceStatusToday { get; set; } = new();
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
}

public class ErpReportDto
{
    public int TotalPayrollRuns { get; set; }
    public decimal TotalPayrollExpenseThisMonth { get; set; }
    public int ActiveContracts { get; set; }
    public int TotalQuotes { get; set; }

    public Dictionary<string, decimal> PayrollExpenseTrend { get; set; } = new();
}

public class TimeTrackingReportDto
{
    public double TotalHoursLoggedThisWeek { get; set; }
    public double TotalHoursLoggedThisMonth { get; set; }
    
    public Dictionary<string, double> HoursByAgent { get; set; } = new();
    public Dictionary<string, double> HoursByProject { get; set; } = new();
}
