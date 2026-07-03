using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Crm;

public class Interaction : BaseEntity
{
    public string Direction { get; set; } = null!; // Inbound, Outbound
    public string Subject { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Channel { get; set; } = null!; // Email, SMS, Portal, etc.
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    
    // Link to Ticket, Case, etc.
    public Guid? RelatedToId { get; set; }
    
    public Guid? LoggedById { get; set; } // Agent
    
    public DateTime OccurredAt { get; set; }
}
