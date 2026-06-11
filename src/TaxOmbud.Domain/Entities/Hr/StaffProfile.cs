using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class StaffProfile : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTimeOffset HireDate { get; set; }
    public string EmploymentStatus { get; set; } = "Active"; // Active, Suspended, Terminated
    
    public DateTimeOffset DateOfBirth { get; set; }
    public string Nationality { get; set; } = "Nigerian";
    public string MaritalStatus { get; set; } = "Single"; // Single, Married, Divorced, Widowed
    
    public string EmergencyContact { get; set; } = null!;
    public string BankAccountNo { get; set; } = null!;
    public string BankId { get; set; } = null!; // Bank code or name
    
    public string NextOfKin { get; set; } = null!;
}
