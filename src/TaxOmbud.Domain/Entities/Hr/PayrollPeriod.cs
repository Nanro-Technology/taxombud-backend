using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayrollPeriod : BaseAuditableEntity
{
    public string Name { get; set; } = null!; // e.g., "May 2026"
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Status { get; set; } = "open"; // open, locked, posted
}
