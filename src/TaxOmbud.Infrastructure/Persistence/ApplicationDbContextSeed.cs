using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Infrastructure.Persistence;

/// <summary>
/// Seeds essential reference data on first startup.
/// Run only when the database has just been created/migrated.
/// </summary>
public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        await SeedRolesAndPermissionsAsync(context, logger);
        await SeedDefaultAdminAsync(context, logger);
        await SeedSettingsAsync(context, logger);
    }

    // ─── Roles & Permissions ──────────────────────────────────────────────────
    private static async Task SeedRolesAndPermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        var roleDefs = new[]
        {
            ("SuperAdmin",   "Full system access"),
            ("Admin",        "Administrative access"),
            ("Officer",      "Case management officer"),
            ("SeniorOfficer","Senior officer with escalation rights"),
            ("Manager",      "Department manager"),
            ("Director",     "Directorate director"),
            ("Taxpayer",     "External taxpayer portal user"),
            ("Auditor",      "Read-only audit access"),
            ("HrManager",    "HR and payroll manager"),
            ("Finance",      "Finance and remittance officer"),
        };

        foreach (var (name, desc) in roleDefs)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == name))
            {
                context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Code = name.ToLowerInvariant(),
                    Description = desc
                });
            }
        }

        var permissionDefs = new[]
        {
            // Complaints
            ("complaints.submit",   "Complaints", "Submit a new complaint"),
            ("complaints.view",     "Complaints", "View complaints"),
            ("complaints.assign",   "Complaints", "Assign complaints to officers"),
            ("complaints.close",    "Complaints", "Close complaints"),
            ("complaints.escalate", "Complaints", "Escalate complaints"),
            ("complaints.reopen",   "Complaints", "Reopen closed complaints"),
            // Cases
            ("cases.view",          "Cases", "View cases"),
            ("cases.create",        "Cases", "Create cases from complaints"),
            ("cases.assign",        "Cases", "Assign cases"),
            ("cases.close",         "Cases", "Close cases"),
            // Users
            ("users.view",          "Users", "View users"),
            ("users.create",        "Users", "Create users"),
            ("users.edit",          "Users", "Edit users"),
            ("users.deactivate",    "Users", "Deactivate users"),
            // Roles
            ("roles.manage",        "Roles", "Manage roles and permissions"),
            // Documents
            ("documents.upload",    "Documents", "Upload documents"),
            ("documents.view",      "Documents", "View documents"),
            ("documents.delete",    "Documents", "Delete documents"),
            // Reports
            ("reports.view",        "Reports", "View reports"),
            ("reports.export",      "Reports", "Export reports"),
            // HR
            ("hr.view",             "HR", "View HR records"),
            ("hr.manage",           "HR", "Manage HR records"),
            ("payroll.run",         "Payroll", "Run payroll"),
            ("payroll.approve",     "Payroll", "Approve payroll"),
            // System
            ("system.settings",     "System", "Manage system settings"),
            ("audit.view",          "Audit", "View audit logs"),
        };

        foreach (var (code, entity, desc) in permissionDefs)
        {
            if (!await context.Permissions.AnyAsync(p => p.Code == code))
            {
                var action = code.Contains('.') ? code.Split('.')[1] : code;
                context.Permissions.Add(new Permission
                {
                    Code = code,
                    Entity = entity,
                    Action = action,
                    Description = desc
                });
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("✓ Roles and permissions seeded");
    }

    // ─── Default SuperAdmin ───────────────────────────────────────────────────
    private static async Task SeedDefaultAdminAsync(ApplicationDbContext context, ILogger logger)
    {
        const string adminEmail = "admin@taxombud.gov.ng";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == "SuperAdmin");

        var admin = User.Create(
            "System",
            "Administrator",
            new Domain.ValueObjects.Email(adminEmail),
            null);

        // Default password — must be changed on first login
        admin.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("Admin@TaxOmbud2025!", 12));
        admin.AddRole(superAdminRole.Id);

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("✓ Default SuperAdmin seeded: {Email}", adminEmail);
        logger.LogWarning("⚠ Change the default admin password immediately after first login!");
    }

    // ─── Default System Settings ──────────────────────────────────────────────
    private static async Task SeedSettingsAsync(ApplicationDbContext context, ILogger logger)
    {
        var settings = new[]
        {
            new TaxOmbud.Domain.Entities.System.SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = "Security:E2EE_Enabled",
                Value = "false", // Default to false
                Description = "Toggles End-to-End Encryption (E2EE) for the API"
            }
        };

        foreach (var setting in settings)
        {
            if (!await context.SystemSettings.AnyAsync(s => s.Key == setting.Key))
            {
                context.SystemSettings.Add(setting);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("✓ System settings seeded");
    }
}
