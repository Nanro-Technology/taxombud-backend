using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Communications;

public class AgentChat : BaseEntity
{
    public string? Topic { get; set; }
    public bool IsGroupChat { get; set; } = false;
    
    // JSON list of participant UserIds
    public string ParticipantIds { get; set; } = "[]";

    public ICollection<AgentChatMessage> Messages { get; set; } = new List<AgentChatMessage>();
}

public class AgentChatMessage : BaseEntity
{
    public Guid AgentChatId { get; set; }
    public AgentChat Chat { get; set; } = null!;

    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    
    public bool IsPinned { get; set; } = false;

    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    
    // JSON array of objects: { "UserId": "guid", "ReadAt": "datetime" }
    public string ReadReceipts { get; set; } = "[]";
}
