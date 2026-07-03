using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayrollRun : BaseEntity
{
    public Guid PeriodId { get; set; }
    public PayrollPeriod Period { get; set; } = null!;

    public string RunType { get; set; } = "regular"; // regular, offcycle
    public string Status { get; set; } = "draft"; // draft, validated, approved, posted

    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalStatutory { get; set; }

    public Guid? ApprovedBy { get; set; }
    public User? ApprovedByUser { get; set; }
    
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
}
