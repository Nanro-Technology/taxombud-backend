using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.System;

public class AnnouncementReadReceipt : BaseEntity
{
    public Guid AnnouncementId { get; set; }
    public Announcement Announcement { get; set; } = null!;
    
    public Guid UserId { get; set; }
    
    public DateTime ReadAt { get; set; }
}
