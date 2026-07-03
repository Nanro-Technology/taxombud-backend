using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Finance;

public class InvoiceItem : BaseEntity
{

    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string? ItemName { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; } // Quantity * UnitPrice
}
