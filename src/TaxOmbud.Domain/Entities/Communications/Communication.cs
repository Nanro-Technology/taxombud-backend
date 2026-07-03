using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Communications;

public class Communication : BaseEntity
{
    public string Recipient { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    
    public string Channel { get; set; } = "email"; // email, sms, inapp
    public CommunicationDirection Direction { get; set; }
    
    public DateTimeOffset? SentAt { get; set; }
    public bool IsSent { get; set; }
    public string? ErrorMessage { get; set; }
}
