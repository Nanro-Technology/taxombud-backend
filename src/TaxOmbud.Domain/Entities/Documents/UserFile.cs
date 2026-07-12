using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Documents;

public class UserFile : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "file" or "folder"
    public string Area { get; set; } = null!; // "MyFiles", "PublicFiles", "Temp"
    public string Path { get; set; } = ""; // E.g., "Compliance Audits" or "" for root

    // File-specific properties
    public string? StorageKey { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? Content { get; set; } // Short preview text content

    public Guid OwnerId { get; set; } // Scopes "My Files" access
}
