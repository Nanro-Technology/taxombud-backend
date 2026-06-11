using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class Announcement : BaseAuditableEntity
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Scope { get; set; } = "Global"; // Global, Department, Role
    
    public Guid? DepartmentId { get; set; }
    public string? TargetRole { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPinned { get; set; } = false;
}
