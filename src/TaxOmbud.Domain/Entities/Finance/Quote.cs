using System;
namespace TaxOmbud.Domain.Entities.Finance;
public class Quote
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? QuoteNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
}
