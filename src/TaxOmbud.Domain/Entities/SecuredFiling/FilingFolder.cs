using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.SecuredFiling;

public class FilingFolder : BaseEntity
{
    public string FolderCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Priority { get; set; } = "normal"; // low, normal, urgent, top_urgent
    public string Confidentiality { get; set; } = "normal"; // normal, confidential, top_secret
    public string Dept { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string IntakeMethod { get; set; } = "internal"; // walk_in, mail, courier, email_print, internal
    public string SenderName { get; set; } = string.Empty;
    public string SenderOrg { get; set; } = string.Empty;
    public string SenderRef { get; set; } = string.Empty;
    public string InternalRef { get; set; } = string.Empty;
    public string Status { get; set; } = "active"; // active, pending_ack, in_progress, closed, rejected
    
    public ICollection<FilingDocument> Documents { get; set; } = new List<FilingDocument>();
    public ICollection<FilingInboxRouting> InboxRoutings { get; set; } = new List<FilingInboxRouting>();
}
