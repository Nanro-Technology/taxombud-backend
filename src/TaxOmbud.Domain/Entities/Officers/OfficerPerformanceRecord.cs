using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Officers;

public class OfficerPerformanceRecord : BaseAuditableEntity
{
    public Guid OfficerId { get; set; }
    public Officer Officer { get; set; } = null!;

    public DateTime Month { get; set; }
    public int CasesResolved { get; set; }
    public int CasesAssigned { get; set; }
    public decimal AverageResolutionTimeDays { get; set; }
    public decimal CsatScore { get; set; }
}
