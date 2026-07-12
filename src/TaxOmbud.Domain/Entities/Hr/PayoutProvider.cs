using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayoutProvider : BaseEntity
{
    public string Name { get; set; } = null!;
    public string ProviderCode { get; set; } = null!;
    public string Type { get; set; } = "Bank";
    public string Adapter { get; set; } = "manual";
    public string Country { get; set; } = "NG";
    public string Currency { get; set; } = "NGN";
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? Notes { get; set; }
    
    public string Status { get; set; } = "Active";
}
