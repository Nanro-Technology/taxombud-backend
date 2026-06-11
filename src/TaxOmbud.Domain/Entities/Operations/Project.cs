using System;
namespace TaxOmbud.Domain.Entities.Operations;
public class Project
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
