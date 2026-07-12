using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.SecuredFiling;

public class FilingInboxRouting : BaseEntity
{
    public Guid FolderId { get; set; }
    public FilingFolder Folder { get; set; } = null!;
    
    public Guid AssignedToUserId { get; set; }
    public string AssignedToDept { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string Status { get; set; } = "to_acknowledge"; // to_acknowledge, in_progress, sent, archive
    public string SentBy { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
}
