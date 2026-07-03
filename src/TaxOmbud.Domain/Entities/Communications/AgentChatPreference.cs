using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

public class AgentChatPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public bool DoNotDisturb { get; set; } = false;
    public bool MarkAsAway { get; set; } = false;
    public bool PlayNotificationSound { get; set; } = true;
    public bool ShowBrowserNotifications { get; set; } = true;
}
