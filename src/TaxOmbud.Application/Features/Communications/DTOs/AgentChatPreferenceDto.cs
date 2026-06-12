using System;

namespace TaxOmbud.Application.Features.Communications.DTOs;

public class AgentChatPreferenceDto
{
    public Guid UserId { get; set; }
    public bool DoNotDisturb { get; set; }
    public bool MarkAsAway { get; set; }
    public bool PlayNotificationSound { get; set; }
    public bool ShowBrowserNotifications { get; set; }
}
