using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Complaints;

public class ComplaintLink : BaseEntity
{
    public Guid SourceComplaintId { get; set; }
    public Complaint SourceComplaint { get; set; } = null!;

    public Guid TargetComplaintId { get; set; }
    public Complaint TargetComplaint { get; set; } = null!;

    public string LinkType { get; set; } = "related"; // e.g. related, duplicate, split
    
    public Guid LinkedByUserId { get; set; }
}
