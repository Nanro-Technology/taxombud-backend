using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class StaffDocument : BaseEntity
{
    public Guid StaffProfileId { get; set; }
    public StaffProfile StaffProfile { get; set; } = null!;
    
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? DocumentType { get; set; } // Resume, ID Card, Certificate
}

public class StaffNote : BaseEntity
{
    public Guid StaffProfileId { get; set; }
    public StaffProfile StaffProfile { get; set; } = null!;

    public string Note { get; set; } = null!;
    public Guid AddedByUserId { get; set; }
}

public class DepartmentMovement : BaseEntity
{
    public Guid StaffProfileId { get; set; }
    public StaffProfile StaffProfile { get; set; } = null!;

    public Guid? FromDepartmentId { get; set; }
    public Guid ToDepartmentId { get; set; }
    
    public DateTime MovementDate { get; set; }
    public string? Reason { get; set; }
}
