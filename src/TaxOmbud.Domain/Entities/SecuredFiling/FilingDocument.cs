using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.SecuredFiling;

public class FilingDocument : BaseEntity
{
    public Guid FolderId { get; set; }
    public FilingFolder Folder { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    public string Size { get; set; } = "0 B";
    public string Type { get; set; } = "PDF";
    public string OcrStatus { get; set; } = "pending"; // pending, done, failed
    public string OcrText { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderOrg { get; set; } = string.Empty;
    public string SenderRef { get; set; } = string.Empty;
    public string InternalRef { get; set; } = string.Empty;
}
