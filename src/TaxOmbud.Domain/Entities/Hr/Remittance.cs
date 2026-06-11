using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class Remittance : BaseAuditableEntity
{
    public Guid RunId { get; set; }
    public PayrollRun Run { get; set; } = null!;

    public string DeductionType { get; set; } = "paye"; // paye, pension, nhf, other
    public decimal Amount { get; set; }
    public string Status { get; set; } = "draft"; // draft, submitted, paid
    public string? ReferenceNumber { get; set; }
}
