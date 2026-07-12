using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Documents;

public class SignRequest : BaseEntity
{
    public string FileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;
    public string Status { get; set; } = "Pending"; // Pending, Signed, Expired
    public string Token { get; set; } = null!;
    public string SignatoryEmail { get; set; } = null!;
    
    public string? SignedFileName { get; set; }
    public string? SignedStorageKey { get; set; }
}
