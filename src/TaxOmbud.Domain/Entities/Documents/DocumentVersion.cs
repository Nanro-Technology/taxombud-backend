using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Documents;

public class DocumentVersion : BaseAuditableEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public Guid UploadedByUserId { get; set; }
}
