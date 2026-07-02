using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Persistence.Data;

/// <summary>
/// Seeds all reference data (permissions, roles, role-permissions, admin user) on startup.
/// Idempotent — safe to run multiple times.
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
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
                        CreatedAt = DateTimeOffset.UtcNow
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
        var roleDefs = new[]
        {
            (RoleConstants.SuperAdmin,    "Full system access with all permissions",    true),
            (RoleConstants.Admin,         "Administrative access",                      true),
            (RoleConstants.Director,      "Directorate director",                       false),
            (RoleConstants.Manager,       "Department manager",                         false),
            (RoleConstants.SeniorOfficer, "Senior officer with escalation rights",      false),
            (RoleConstants.Officer,       "Case management officer",                    false),
            (RoleConstants.Taxpayer,      "External taxpayer portal user",              false),
            (RoleConstants.Auditor,       "Read-only audit access",                     false),
            (RoleConstants.HrManager,     "HR and payroll manager",                     false),
            (RoleConstants.Finance,       "Finance and remittance officer",             false),
        };

        foreach (var (name, description, isSystem) in roleDefs)
        {
            if (!await _context.Roles.AnyAsync(r => r.Name == name))
            {
                await _context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    IsSystemRole = isSystem,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
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
        var superAdmin = await _context.Roles
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
                    CreatedAt = DateTimeOffset.UtcNow
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
    private async Task SeedUsersAsync()
    {
        const string adminEmail = "admin@taxombud.gov.ng";

        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.SuperAdmin);
        if (superAdminRole is null) return;

        var admin = User.Create(
            "System",
            "Administrator",
            new Domain.ValueObjects.Email(adminEmail),
            null);

        // Default password — must be changed on first login
        admin.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("Admin@TaxOmbud2025!", 12));
        admin.AssignRole(superAdminRole.Id);

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✓ Default Super Admin seeded: {Email}", adminEmail);
        _logger.LogWarning("⚠ Change the default admin password immediately after first login!");
    }
}
