using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Finance;

public class Quote : BaseAuditableEntity
{
    public string? QuoteNumber { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; } = "Draft"; // Draft, Sent, Approved, Rejected

    public string Currency { get; set; } = "NGN";
    public string? ParentType { get; set; } // Account, Organization
    public Guid? ParentId { get; set; }
    
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string? Notes { get; set; }
    
    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}

public class QuoteItem : BaseAuditableEntity
{
    public Guid QuoteId { get; set; }
    public Quote Quote { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
