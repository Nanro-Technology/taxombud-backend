using System;

namespace TaxOmbud.Application.Features.Tasks.DTOs;

public class CaseTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    
    public DateTimeOffset? DueAt { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? LinkedCaseId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
