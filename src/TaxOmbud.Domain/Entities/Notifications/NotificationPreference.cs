using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Notifications;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string EventType { get; set; } = null!; // e.g. complaint.submitted, case.assigned
    
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
}
