using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class WebhookSubscription : BaseAuditableEntity
{
    public string Url { get; set; } = null!;
    public string Secret { get; set; } = null!; // HMAC-SHA256 signing secret key
    
    public string EventTypes { get; set; } = null!; // Comma-separated list of event types, e.g. "complaint.submitted,case.opened"
    public bool IsActive { get; set; } = true;
}
