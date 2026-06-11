using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Complaints;

public class ComplaintNote : BaseAuditableEntity
{
    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = null!;
    
    public string Visibility { get; set; } = "internal"; // internal or external
}
