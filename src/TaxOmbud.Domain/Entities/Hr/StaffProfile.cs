using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Domain.Entities.Hr;

public class StaffProfile : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? EmployeeCode { get; set; }
    public string? Title { get; set; } // Dr, Engr, Prof
    public Guid? SupervisorId { get; set; }

    public DateTimeOffset HireDate { get; set; }
    public string EmploymentStatus { get; set; } = "Active"; // Active, Suspended, Terminated
    
    public DateTimeOffset DateOfBirth { get; set; }
    public string Nationality { get; set; } = "Nigerian";
    public string MaritalStatus { get; set; } = "Single"; // Single, Married, Divorced, Widowed
    
    // Education
    public string? EducationLevel { get; set; }
    public string? EducationDetails { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    public string? NextOfKinName { get; set; }
    public string? NextOfKinRelationship { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinAddress { get; set; }

    public string BankAccountNo { get; set; } = null!;
    public string BankId { get; set; } = null!; // Bank code or name

    public ICollection<StaffDocument> Documents { get; set; } = new List<StaffDocument>();
    public ICollection<StaffNote> Notes { get; set; } = new List<StaffNote>();
    public ICollection<DepartmentMovement> Movements { get; set; } = new List<DepartmentMovement>();
}
