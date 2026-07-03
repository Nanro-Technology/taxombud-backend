using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class DashboardWidget : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ComponentName { get; set; } = null!; // The frontend component key (e.g. 'open-cases')
    public string? RequiredPermission { get; set; } // Role/Policy required to use this widget
    
    public bool IsActive { get; set; } = true;
}

public class UserDashboardLayout : BaseEntity
{
    public Guid UserId { get; set; }
    
    public string LayoutJson { get; set; } = "[]"; // Serialized layout grid data
}
