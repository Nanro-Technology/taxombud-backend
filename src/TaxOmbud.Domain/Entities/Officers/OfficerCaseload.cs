using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Officers;

/// <summary>Tracks active case assignments for workload balancing.</summary>
public class OfficerCaseload : BaseAuditableEntity
{
    public Guid OfficerProfileId { get; set; }
    public OfficerProfile OfficerProfile { get; set; } = null!;

    public Guid CaseId { get; set; }       // FK to Cases.Case
    public bool IsActive { get; set; } = true;
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}
