using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Communications;

/// <summary>Immutable record of every communication sent or received.</summary>
public class CommunicationLog : BaseAuditableEntity
{
    public Guid? RelatedEntityId { get; set; }         // FK to complaint / case / appeal
    public string? RelatedEntityType { get; set; }     // "Complaint", "Case", "Appeal"

    public string Recipient { get; set; } = null!;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;

    public string Channel { get; set; } = "email";    // email | sms | inapp | letter
    public CommunicationDirection Direction { get; set; } = CommunicationDirection.Outbound;

    public DateTimeOffset? SentAt { get; set; }
    public bool IsSent { get; set; } = false;
    public string? ErrorMessage { get; set; }

    public Guid? SentByUserId { get; set; }
}
