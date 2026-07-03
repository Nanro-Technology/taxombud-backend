using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Notifications;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
