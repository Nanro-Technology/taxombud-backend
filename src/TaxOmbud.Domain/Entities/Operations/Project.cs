using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Operations;

public class Project : BaseAuditableEntity
{
    public string? Name { get; set; } // Note: "Title" in form maps to Name
    public string? Description { get; set; }
    public string? Status { get; set; } // Planning, Active, Paused, Completed
    
    public DateTime? StartDate { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid? OwnerId { get; set; }
}

public class ProjectMember : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid UserId { get; set; }
}
