using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Persistence.Data;

/// <summary>
/// Seeds all reference data (permissions, roles, role-permissions, admin user) on startup.
/// Idempotent — safe to run multiple times.
/// Now uses UserManager<User> to create the seed user so that Identity's password hashing,
/// normalisation, and security stamp are all properly initialised.
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
        await SeedDepartmentsAsync();
        await SeedUsersAsync();
    }

    // ─── 1. Permissions ──────────────────────────────────────────────────────────
    /// <summary>
    /// Auto-generates one Permission row for every Modules × PermissionAction combination.
    /// New modules or actions added to the enums will be seeded on next startup.
    /// </summary>
    private async Task SeedPermissionsAsync()
    {
        var existing = await _context.Permissions.ToListAsync();
        var toAdd = new List<Permission>();

        foreach (Modules module in Enum.GetValues(typeof(Modules)))
        {
            foreach (PermissionAction action in Enum.GetValues(typeof(PermissionAction)))
            {
                if (!existing.Any(p => p.Module == module && p.Action == action))
                {
                    toAdd.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Module = module,
                        Action = action,
                        CreatedAt = DateTime.Now.ToUniversalTime()
                    });
                }
            }
        }

        if (toAdd.Any())
        {
            await _context.Permissions.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded {Count} new permissions", toAdd.Count);
        }
    }

    // ─── 2. Roles ────────────────────────────────────────────────────────────────
    private async Task SeedRolesAsync()
    {
        // NOTE: These roles are ONLY for StaffUser accounts.
        // Taxpayers and Guests do NOT have roles — their UserType is their identity.
        var roleDefs = new[]
        {
            (RoleConstants.SuperAdmin,    "Full system access with all permissions",    true),
            (RoleConstants.Admin,         "Administrative access",                      true),
            (RoleConstants.Director,      "Directorate director",                       false),
            (RoleConstants.Manager,       "Department manager",                         false),
            (RoleConstants.SeniorOfficer, "Senior officer with escalation rights",      false),
            (RoleConstants.Officer,       "Case management officer",                    false),
            (RoleConstants.Auditor,       "Read-only audit access",                     false),
            (RoleConstants.HrManager,     "HR and payroll manager",                     false),
            (RoleConstants.Finance,       "Finance and remittance officer",             false),
        };

        foreach (var (name, description, isSystem) in roleDefs)
        {
            if (!await _context.CustomRoles.AnyAsync(r => r.Name == name))
            {
                await _context.CustomRoles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    IsSystemRole = isSystem,
                    IsActive = true,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✓ Roles seeded");
    }

    // ─── 3. Role → Permission assignments ────────────────────────────────────────
    /// <summary>Assigns all permissions to SuperAdmin; other roles start with no permissions (configured via UI).</summary>
    private async Task SeedRolePermissionsAsync()
    {
        var superAdmin = await _context.CustomRoles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.SuperAdmin);

        if (superAdmin is null) return;

        var allPermissions = await _context.Permissions.ToListAsync();
        var toAdd = new List<RolePermission>();

        foreach (var permission in allPermissions)
        {
            if (!superAdmin.RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            {
                toAdd.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = superAdmin.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                });
            }
        }

        if (toAdd.Any())
        {
            await _context.RolePermissions.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Assigned {Count} permissions to Super Admin", toAdd.Count);
        }
    }

    // ─── 4. Default Super Admin user ─────────────────────────────────────────────
    /// <summary>
    /// Creates the seeded Super Admin using UserManager so that Identity's password hashing,
    /// security stamp, normalized email, and lockout fields are all correctly initialised.
    /// The first-login password must be changed immediately.
    /// </summary>
    private async Task SeedUsersAsync()
    {
        const string adminEmail = "admin@taxombud.gov.ng";
        const string defaultPassword = "Admin@TaxOmbud2025!";

        var existingAdmin = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var ictDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "ICT");

        if (existingAdmin is not null)
        {
            bool needsUpdate = false;
            // Backfill normalized fields for existing records
            if (string.IsNullOrEmpty(existingAdmin.NormalizedEmail) || string.IsNullOrEmpty(existingAdmin.NormalizedUserName))
            {
                existingAdmin.NormalizedEmail = _userManager.NormalizeEmail(adminEmail);
                existingAdmin.NormalizedUserName = _userManager.NormalizeName(adminEmail);
                needsUpdate = true;
                _logger.LogInformation("✓ Backfilled normalized email/username fields for: {Email}", adminEmail);
            }

            // Backfill default profile details if empty
            if (string.IsNullOrEmpty(existingAdmin.Phone))
            {
                existingAdmin.Phone = "+2349052129949";
                needsUpdate = true;
            }
            if (string.IsNullOrEmpty(existingAdmin.JobTitle))
            {
                existingAdmin.JobTitle = "S.A on ICT";
                needsUpdate = true;
            }
            if (string.IsNullOrEmpty(existingAdmin.EmploymentType))
            {
                existingAdmin.EmploymentType = "Full-Time";
                needsUpdate = true;
            }
            if (existingAdmin.DepartmentId == null && ictDept is not null)
            {
                existingAdmin.DepartmentId = ictDept.Id;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _userManager.UpdateAsync(existingAdmin);
                _logger.LogInformation("✓ Backfilled seeded admin profile details: Phone, Job Title, Employment Type, Department.");
            }
            return;
        }

        var superAdminRole = await _context.CustomRoles.FirstOrDefaultAsync(r => r.Name == RoleConstants.SuperAdmin);
        if (superAdminRole is null)
        {
            _logger.LogError("SuperAdmin role not found during seeding. Cannot create default admin.");
            return;
        }

        var admin = User.Create(
            "System",
            "Administrator",
            new Email(adminEmail),
            "+2349052129949",
            UserType.StaffUser);

        admin.AssignRole(superAdminRole.Id);
        admin.UpdateProfile("System", "Administrator", "+2349052129949", "S.A on ICT");
        admin.SetEmploymentType("Full-Time");
        if (ictDept is not null)
        {
            admin.SetDepartment(ictDept.Id);
        }

        // Use UserManager to create the admin — Identity handles hashing & security stamp
        var result = await _userManager.CreateAsync(admin, defaultPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("✓ Default Super Admin seeded: {Email}", adminEmail);
            _logger.LogWarning("⚠ Change the default admin password immediately after first login!");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("✗ Failed to seed Super Admin: {Errors}", errors);
        }
    }

    // ─── 5. Default Departments ──────────────────────────────────────────────────
    private async Task SeedDepartmentsAsync()
    {
        var existing = await _context.Departments.AnyAsync();
        if (!existing)
        {
            var depts = new[]
            {
                new Department { Id = Guid.NewGuid(), Name = "ICT", RoutingMode = "members", Description = "Information and Communication Technology department" },
                new Department { Id = Guid.NewGuid(), Name = "Resolution", RoutingMode = "members", Description = "Case resolution and mediation department" },
                new Department { Id = Guid.NewGuid(), Name = "Corporate HQ", RoutingMode = "members", Description = "Corporate Headquarters" }
            };
            await _context.Departments.AddRangeAsync(depts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✓ Seeded default departments");
        }
    }
}
