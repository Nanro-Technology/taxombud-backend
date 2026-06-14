using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Operations;

public class Visitor : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    
    public string VisitorCode { get; set; } = null!;
    public Guid HostId { get; set; }
    
    public DateTime ExpectedArrival { get; set; }
    
    public string Status { get; set; } = "Expected"; // Expected, Checked-In, Checked-Out
    public Guid? RequestedById { get; set; }
}
