using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Documents;

public class PublicFileRequestUpload : BaseEntity
{
    public Guid PublicFileRequestId { get; set; }
    public PublicFileRequest PublicFileRequest { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public long FileSize { get; set; }
}
