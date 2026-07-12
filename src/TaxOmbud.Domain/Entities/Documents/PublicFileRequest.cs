using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Documents;

public class PublicFileRequest : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired
    
    public string AllowedExtensions { get; set; } = "pdf"; // Comma-separated, e.g. "pdf,png"
    public int MaxSizeMb { get; set; } = 10;
    public string NotifyEmails { get; set; } = null!;
    public string? Notes { get; set; }

    public ICollection<PublicFileRequestUpload> Uploads { get; set; } = new List<PublicFileRequestUpload>();
}
