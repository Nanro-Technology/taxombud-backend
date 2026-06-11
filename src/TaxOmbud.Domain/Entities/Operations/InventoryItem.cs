using System;
namespace TaxOmbud.Domain.Entities.Operations;
public class InventoryItem
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? Name { get; set; }
    public string? SKU { get; set; }
    public int Quantity { get; set; }
}
