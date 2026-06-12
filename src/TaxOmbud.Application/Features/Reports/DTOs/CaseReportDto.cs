using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Reports.DTOs;

public class CaseReportDto
{
    public int TotalCases { get; set; }
    public int OpenCases { get; set; }
    public int ClosedCases { get; set; }
    public int EscalatedCases { get; set; }

    public Dictionary<string, int> CasesByStatus { get; set; } = new();
    public Dictionary<string, int> CasesByPriority { get; set; } = new();
    public Dictionary<string, int> CasesByCategory { get; set; } = new();

    // Aging: "0-2 days", "3-7 days", "8-14 days", "15+ days"
    public Dictionary<string, int> AgingBuckets { get; set; } = new();
}
