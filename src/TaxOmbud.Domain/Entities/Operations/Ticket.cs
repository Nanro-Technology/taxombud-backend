using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Operations;

public class Ticket : BaseAuditableEntity
{
    public string TicketNumber { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid SenderId { get; set; }
    public Guid? SenderDepartmentId { get; set; }
    
    public Guid? AssignedDepartmentId { get; set; }
    public Guid? DestinationUserId { get; set; }
    
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
}
