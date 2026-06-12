namespace TaxOmbud.Application.Features.Reports.DTOs;

public class SlaReportDto
{
    public int TotalCasesMeasured { get; set; }
    public int CasesWithinSla { get; set; }
    public int CasesBreachedSla { get; set; }
    public double SlaCompliancePercentage { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}
