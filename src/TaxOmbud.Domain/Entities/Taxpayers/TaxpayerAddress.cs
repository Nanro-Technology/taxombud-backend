using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Taxpayers;

public class TaxpayerAddress : BaseEntity
{
    public Guid TaxpayerId { get; set; }
    public Taxpayer Taxpayer { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Nigeria";
}
