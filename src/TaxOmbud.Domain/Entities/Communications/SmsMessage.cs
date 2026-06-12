using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

public class SmsMessage : BaseAuditableEntity
{
    public string Provider { get; set; } = null!;
    public string? SenderId { get; set; }
    public string Body { get; set; } = null!;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string RecipientType { get; set; } = null!;
    public string? PhoneNumbers { get; set; }
    public string Mode { get; set; } = null!; // Single, Bulk
    public string Status { get; set; } = null!; // Pending, Sent, Failed
    public string Direction { get; set; } = null!; // Inbound, Outbound
}
