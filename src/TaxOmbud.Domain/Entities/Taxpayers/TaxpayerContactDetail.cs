using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Taxpayers;

public class TaxpayerContactDetail : BaseAuditableEntity
{
    public Guid TaxpayerId { get; set; }
    public Taxpayer Taxpayer { get; set; } = null!;

    public string PrimaryEmail { get; set; } = null!;
    public string PrimaryPhone { get; set; } = null!;
    public string? AlternativePhone { get; set; }
    public string PreferredContactMethod { get; set; } = "email"; // email, phone, sms
}
