using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    
    public string Action { get; set; } = null!; // Create, Update, Delete, Login, Impersonate
    
    public string? OldValues { get; set; } // JSON format
    public string? NewValues { get; set; } // JSON format
    
    public Guid? UserId { get; set; }
    public Guid? ImpersonatorUserId { get; set; } // Tracks who performed action under impersonation
    
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}
