using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Complaints;

public class CallCenterRecord : BaseEntity
{
    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public string CallerName { get; set; } = null!;
    public string CallerPhoneNumber { get; set; } = null!;
    public string HotlineLineUsed { get; set; } = null!; // MTN (0814 589 5508), Airtel (0708 268 4497), Glo (0905 014 0904)
    public int DurationSeconds { get; set; }
    public string? RecordingFileUrl { get; set; }
    public string CallSummary { get; set; } = null!;

    public Guid LoggedByAgentId { get; set; }
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
}
