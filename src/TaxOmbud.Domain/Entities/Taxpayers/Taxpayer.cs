using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Domain.Entities.Taxpayers;

public class Taxpayer : BaseAuditableEntity
{
    public Guid AccountId { get; set; } // Workflow Lane
    public Account Account { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    
    public Email Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? AltPhone { get; set; }
    public string? Gender { get; set; }
    
    public string? Nin { get; set; }
    public string? Bvn { get; set; }
    public TaxIdentificationNumber? TaxId { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }

    public string PasswordHash { get; set; } = null!; // Taxpayer portal credential
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;
}
