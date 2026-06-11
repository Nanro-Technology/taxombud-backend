using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Officers;

public class Officer : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int MaxCaseload { get; set; } = 50;
    public bool IsAvailable { get; set; } = true;

    public ICollection<OfficerPerformanceRecord> PerformanceRecords { get; set; } = new List<OfficerPerformanceRecord>();
}
