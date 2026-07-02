namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// String constants for the pre-seeded system role names.
/// Use these constants everywhere role names are compared or assigned
/// to prevent typos and magic strings.
/// </summary>
public static class RoleConstants
{
    public const string SuperAdmin    = "Super Admin";
    public const string Admin         = "Admin";
    public const string Director      = "Director";
    public const string Manager       = "Manager";
    public const string SeniorOfficer = "Senior Officer";
    public const string Officer       = "Officer";
    public const string Taxpayer      = "Taxpayer";
    public const string Auditor       = "Auditor";
    public const string HrManager     = "HR Manager";
    public const string Finance       = "Finance";
}
