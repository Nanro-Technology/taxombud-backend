namespace TaxOmbud.Domain.Entities.Identity;

/// <summary>
/// String constants for the pre-seeded system role names.
/// These roles are EXCLUSIVELY for StaffUser accounts.
/// Taxpayers and Guests are identified by their UserType — they do NOT have roles.
///
/// Use these constants everywhere role names are compared or assigned
/// to prevent typos and magic strings.
/// </summary>
public static class RoleConstants
{
    // ─── System roles (IsSystemRole = true, cannot be deleted) ───────────────
    public const string SuperAdmin    = "Super Admin";   // All permissions — first admin seeded on startup
    public const string Admin         = "Admin";         // Full admin access, assigned by SuperAdmin

    // ─── Staff roles (IsSystemRole = false, manageable via UI) ───────────────
    public const string Director      = "Director";
    public const string Manager       = "Manager";
    public const string SeniorOfficer = "Senior Officer";
    public const string Officer       = "Officer";
    public const string Auditor       = "Auditor";
    public const string HrManager     = "HR Manager";
    public const string Finance       = "Finance";

    // ─── Helper: all staff role names (for validation) ───────────────────────
    public static readonly IReadOnlyList<string> AllStaffRoles = new[]
    {
        SuperAdmin, Admin, Director, Manager, SeniorOfficer, Officer, Auditor, HrManager, Finance
    };
}
