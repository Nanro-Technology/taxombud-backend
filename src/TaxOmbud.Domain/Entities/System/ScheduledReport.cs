using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class ScheduledReport : BaseAuditableEntity
{
    public string ReportName { get; set; } = null!;
    public string CronExpression { get; set; } = null!; // e.g. "0 6 * * *" for daily at 6:00
    
    public string Recipients { get; set; } = null!; // Comma-separated email list
    public string Format { get; set; } = "CSV"; // CSV, Excel, PDF
    
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastRunAt { get; set; }
}
