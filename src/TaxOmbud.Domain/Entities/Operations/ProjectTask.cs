using System;
namespace TaxOmbud.Domain.Entities.Operations;
public class ProjectTask
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
}
