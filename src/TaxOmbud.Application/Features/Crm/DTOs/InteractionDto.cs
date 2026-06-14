using System;

namespace TaxOmbud.Application.Features.Crm.DTOs;

public class InteractionDto
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Channel { get; set; } = null!;
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    public Guid? RelatedToId { get; set; }
    public Guid? LoggedById { get; set; }
    public DateTime OccurredAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
