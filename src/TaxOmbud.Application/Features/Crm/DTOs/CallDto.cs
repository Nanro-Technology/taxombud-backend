using System;

namespace TaxOmbud.Application.Features.Crm.DTOs;

public class CallDto
{
    public Guid Id { get; set; }
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
    
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
