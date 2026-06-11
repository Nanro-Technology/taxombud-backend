using System;
namespace TaxOmbud.Domain.Entities.Finance;
public class Contract
{

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? ContractNumber { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
}
