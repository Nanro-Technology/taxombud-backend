using System;

namespace TaxOmbud.Application.Features.Reports.DTOs;

public class ReportFilterDto
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}
