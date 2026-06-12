using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

public class MailboxMessage : BaseAuditableEntity
{
    public Guid SenderId { get; set; }
    
    public string Subject { get; set; } = null!;
    public string BodyText { get; set; } = null!;
    
    public bool IsDraft { get; set; } = false;
    public string Category { get; set; } = "Primary"; // Primary, Social, Promotions

    public ICollection<MailboxRecipient> Recipients { get; set; } = new List<MailboxRecipient>();
    public ICollection<MailboxAttachment> Attachments { get; set; } = new List<MailboxAttachment>();
}

public class MailboxRecipient : BaseAuditableEntity
{
    public Guid MessageId { get; set; }
    public MailboxMessage Message { get; set; } = null!;
    
    public Guid RecipientId { get; set; }
    
    public string Type { get; set; } = "To"; // To, Cc, Bcc
    public string Folder { get; set; } = "Inbox"; // Inbox, Archive, Trash, Spam
    
    public bool IsRead { get; set; } = false;
    public bool IsStarred { get; set; } = false;
}

public class MailboxAttachment : BaseAuditableEntity
{
    public Guid MessageId { get; set; }
    public MailboxMessage Message { get; set; } = null!;
    
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public long FileSize { get; set; }
}
