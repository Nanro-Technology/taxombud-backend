using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Hr;

public class PayGrade : BaseEntity
{
    public string Name { get; set; } = null!; // Grade name/level
    public int Level { get; set; } // Numerical order
    public string BasicSalaryBand { get; set; } = null!; // Range details, e.g. "50,000 - 80,000"
    public string Currency { get; set; } = "NGN";
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string? Description { get; set; }
}
