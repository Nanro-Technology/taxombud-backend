using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Roles.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class RolesService : IRolesService
{
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IGenericRepository<Permission> _permissionRepo;
    private readonly IGenericRepository<RolePermission> _rolePermissionRepo;

    public RolesService(
        IGenericRepository<Role> roleRepo,
        IGenericRepository<Permission> permissionRepo,
        IGenericRepository<RolePermission> rolePermissionRepo)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _rolePermissionRepo = rolePermissionRepo;
    }

    // ─── Create Role ──────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a new custom staff role WITH the initial set of module+permission assignments.
    /// A role MUST have at least one permission — an empty role is not allowed.
    /// Only applies to StaffUser accounts; Taxpayer/Guest users do not use roles.
    /// </summary>
    public async Task<Response<CreateRoleResponse>> CreateRoleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateRoleResponse>();

        try
        {
            // Validate: at least one permission must be assigned on creation
            if (request.PermissionIds == null || !request.PermissionIds.Any())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.RoleRequiresPermissions;
                return response;
            }

            var distinctPermissionIds = request.PermissionIds.Distinct().ToList();

            // Validate: role name must be unique
            if (await _roleRepo.ExistsAsync(r => r.Name == request.Name))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "A role with this name already exists.";
                return response;
            }

            // Validate: all supplied permission IDs must exist
            var resolvedPermissions = new List<Permission>();
            foreach (var permId in distinctPermissionIds)
            {
                var permission = await _permissionRepo.GetByIdAsync(permId);
                if (permission is null)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = $"Permission with ID '{permId}' does not exist.";
                    return response;
                }
                resolvedPermissions.Add(permission);
            }

            // Create the role
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsSystemRole = false,   // Custom roles created via API are never system roles
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveAsync();

            // Assign all permissions in one step
            var rolePermissions = resolvedPermissions.Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionId = p.Id,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _rolePermissionRepo.AddRangeAsync(rolePermissions);
            await _rolePermissionRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status201Created;
            response.Message = "Role created successfully.";
            response.Data = new CreateRoleResponse(
                role.Id,
                role.Name,
                role.Description,
                resolvedPermissions.Select(p => new PermissionDto(p.Id, p.Module.ToString(), p.Action.ToString())).ToList()
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    // ─── Update Role Permissions ──────────────────────────────────────────────
    /// <summary>
    /// Replaces the full set of permissions on an existing role.
    /// System roles (SuperAdmin, Admin) can have permissions updated but cannot be deleted.
    /// At least one permission must remain after the update.
    /// </summary>
    public async Task<Response<object?>> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();

        var role = await _roleRepo.Query()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Role not found." };

        try
        {
            // Enforce: cannot leave a role with zero permissions
            if (request.PermissionIds == null || !request.PermissionIds.Any())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.RoleRequiresPermissions;
                return response;
            }

            var distinctPermissionIds = request.PermissionIds.Distinct().ToList();

            // Validate all permission IDs before touching the database
            var resolvedPermissions = new List<Permission>();
            foreach (var permId in distinctPermissionIds)
            {
                var permission = await _permissionRepo.GetByIdAsync(permId);
                if (permission is null)
                    return new Response<object?> { StatusCode = StatusCodes.Status400BadRequest, Message = $"Permission with ID '{permId}' does not exist." };

                resolvedPermissions.Add(permission);
            }

            // Replace the permission set atomically:
            // 1. Remove existing permissions and save changes first to flush SQL DELETEs
            if (role.RolePermissions.Any())
            {
                await _rolePermissionRepo.RemoveRangeAsync(role.RolePermissions);
                await _rolePermissionRepo.SaveAsync();
            }

            // 2. Insert new distinct permission assignments
            var newRolePermissions = resolvedPermissions.Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = request.RoleId,
                PermissionId = p.Id,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _rolePermissionRepo.AddRangeAsync(newRolePermissions);
            await _rolePermissionRepo.SaveAsync();

            return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Role permissions updated successfully." };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    // ─── Get All Permissions ──────────────────────────────────────────────────
    /// <summary>
    /// Returns every available Module × Action permission.
    /// The admin uses this list to build the permission-picker UI when creating/editing a role.
    /// </summary>
    public async Task<Response<IEnumerable<PermissionDetailDto>>> GetPermissionsAsync(GetPermissionsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<PermissionDetailDto>>();
        try
        {
            var permissions = await _permissionRepo.Query()
                .AsNoTracking()
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .Select(p => new PermissionDetailDto(
                    p.Id,
                    p.Module.ToString(),
                    p.Action.ToString()))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
            response.Data = permissions;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    // ─── Get Role By Id ───────────────────────────────────────────────────────
    public async Task<Response<RoleDetailDto>> GetRoleByIdAsync(GetRoleByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RoleDetailDto>();

        var role = await _roleRepo.Query()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
            return new Response<RoleDetailDto> { StatusCode = StatusCodes.Status404NotFound, Message = "Role not found." };

        try
        {
            var dto = new RoleDetailDto(
                role.Id,
                role.Name,
                role.Description,
                role.IsSystemRole,
                role.IsActive,
                role.RolePermissions.Select(rp => new PermissionDto(
                    rp.Permission!.Id,
                    rp.Permission.Module.ToString(),
                    rp.Permission.Action.ToString()
                ))
            );

            return new Response<RoleDetailDto> { StatusCode = StatusCodes.Status200OK, Message = Constants.Messages.Success, Data = dto };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    // ─── Get All Roles ────────────────────────────────────────────────────────
    /// <summary>
    /// Returns all roles. Only staff roles exist — no "Taxpayer" role.
    /// The frontend should display these in the staff role-picker.
    /// </summary>
    public async Task<Response<IEnumerable<RoleDto>>> GetRolesAsync(GetRolesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<RoleDto>>();
        try
        {
            var roles = await _roleRepo.Query()
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsSystemRole, r.IsActive))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Success;
            response.Data = roles;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }
}
