using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class WebhookDelivery : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public WebhookSubscription Subscription { get; set; } = null!;

    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!; // JSON payload
    public string Signature { get; set; } = null!; // Hex encoded HMAC signature

    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public int? HttpStatusCode { get; set; }
    public string? ResponsePayload { get; set; }
    
    public int AttemptCount { get; set; }
    public bool IsSuccess { get; set; }
}
