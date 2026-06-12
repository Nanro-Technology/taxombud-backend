using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class Remittance : BaseAuditableEntity
{
    public Guid RunId { get; set; }
    public PayrollRun Run { get; set; } = null!;

    public string DeductionType { get; set; } = "paye"; // paye, pension, nhf, other
    
    // Original amount
    public decimal Amount { get; set; }
    
    // Extensions for Phase 9
    public decimal? EmployeeTotal { get; set; }
    public decimal? EmployerTotal { get; set; }
    public decimal? TotalPayable { get; set; }
    
    public string Status { get; set; } = "draft"; // draft, submitted, paid
    public string? ReferenceNumber { get; set; }
}
