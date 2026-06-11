namespace TaxOmbud.Domain.Entities.Identity;

public class Permission
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Entity { get; set; } = null!;
    public string Action { get; set; } = null!;
}
