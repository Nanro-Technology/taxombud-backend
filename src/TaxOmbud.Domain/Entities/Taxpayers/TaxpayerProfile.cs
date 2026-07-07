using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Domain.Entities.Taxpayers;

/// <summary>Extended taxpayer profile linked to a portal User account.</summary>
public class TaxpayerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public TaxpayerType TaxpayerType { get; set; } = TaxpayerType.Individual;
    public string? TinNumber { get; set; }
    public string? Nin { get; set; }
    public string? Bvn { get; set; }

    // Individual fields
    public string? Gender { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }

    // Corporate fields
    public string? CompanyName { get; set; }
    public string? RcNumber { get; set; }        // CAC registration number

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public bool IsVerified { get; set; } = false;

    public static TaxpayerProfile Create(Guid userId, string taxpayerType) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaxpayerType = Enum.TryParse<TaxpayerType>(taxpayerType, out var t) ? t : TaxpayerType.Individual
        };
}
