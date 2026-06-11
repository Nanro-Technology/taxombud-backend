using System;
using System.Collections.Generic;
namespace TaxOmbud.Domain.Entities.Finance;
public class Invoice
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? InvoiceNumber { get; set; }
    public string? InvoiceTitle { get; set; }
    public string Currency { get; set; } = "NGN";
    public string? ParentType { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string? Notes { get; set; }
    public string? Status { get; set; } // Draft, Sent, Paid, etc.

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
