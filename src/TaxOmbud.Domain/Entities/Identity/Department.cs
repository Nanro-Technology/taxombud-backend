using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Identity;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
    
    public Guid? HeadUserId { get; set; }
    public User? HeadUser { get; set; }

    public string RoutingMode { get; set; } = "members"; // "head" or "members"
    
    public string? Description { get; set; }
}
