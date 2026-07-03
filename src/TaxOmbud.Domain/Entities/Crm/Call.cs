using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Crm;

public class Call : BaseEntity
{
    public string Subject { get; set; } = null!;
    
    public string? CallerType { get; set; }
    public string? CallerMethod { get; set; }
    public string? CallerIdentifier { get; set; }
    
    public string? CalleeMethod { get; set; }
    public string? CalleeIdentifier { get; set; }
    
    public string Direction { get; set; } = null!; // Inbound, Outbound
    public string Status { get; set; } = null!; // Missed, Completed, Voicemail
    public string Phone { get; set; } = null!;
    
    public string? Notes { get; set; }
    
    public Guid? LinkedToId { get; set; }
    public Guid? AgentId { get; set; }
    
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
