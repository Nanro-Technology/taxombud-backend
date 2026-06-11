using System;
namespace TaxOmbud.Domain.Entities.Finance;
public class InvoiceItem
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}
