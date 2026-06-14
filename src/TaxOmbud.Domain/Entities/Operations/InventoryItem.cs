using System;
namespace TaxOmbud.Domain.Entities.Operations;
public class InventoryItem
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? SKU { get; set; }
    
    public Guid? DepartmentId { get; set; }
    public Guid? AssignedUserId { get; set; }
    
    public string? Location { get; set; }
    public string? Mode { get; set; }
    public int Quantity { get; set; }
    public string? SerialNumber { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
}
