using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Cases;

public class CaseCommunicationLog : BaseAuditableEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string Sender { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public CommunicationDirection Direction { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public string Channel { get; set; } = "email"; // email, sms, letter, call
}
