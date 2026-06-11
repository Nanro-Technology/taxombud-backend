using System;
using System.Collections.Generic;
namespace TaxOmbud.Domain.Entities.Finance;
public class Invoice
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? InvoiceNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
