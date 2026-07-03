using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Officers;

/// <summary>Extended profile data for staff members who handle cases.</summary>
public class OfficerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? EmployeeNumber { get; set; }
    public string? Specialisation { get; set; }   // e.g. VAT, PIT, CIT
    public int MaxCaseload { get; set; } = 50;
    public int CurrentCaseload { get; set; } = 0;
    public bool IsAvailable { get; set; } = true;

    public static OfficerProfile Create(Guid userId) =>
        new() { Id = Guid.NewGuid(), UserId = userId };
}
