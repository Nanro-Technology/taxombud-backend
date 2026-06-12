using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Reports.DTOs;

public class AgentPerformanceDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = null!;
    public int CasesAssigned { get; set; }
    public int CasesResolved { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public int InteractionsHandled { get; set; }
}

public class AgentReportDto
{
    public List<AgentPerformanceDto> AgentPerformances { get; set; } = new();
}
