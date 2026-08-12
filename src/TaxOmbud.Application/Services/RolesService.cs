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
    private readonly IGenericRepository<User> _userRepo;

    public RolesService(
        IGenericRepository<Role> roleRepo,
        IGenericRepository<Permission> permissionRepo,
        IGenericRepository<RolePermission> rolePermissionRepo,
        IGenericRepository<User> userRepo)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _rolePermissionRepo = rolePermissionRepo;
        _userRepo = userRepo;
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

            // Validate: all supplied permission IDs must exist via single batch query
            var resolvedPermissions = await _permissionRepo.Query()
                .Where(p => distinctPermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (resolvedPermissions.Count != distinctPermissionIds.Count)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "One or more permission IDs are invalid or do not exist.";
                return response;
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

    // ─── Update Role (Dynamic Roles) ─────────────────────────────────────────
    /// <summary>
    /// Updates an existing role's name, description, or active status.
    /// System roles cannot be renamed or deactivated.
    /// </summary>
    public async Task<Response<RoleDetailDto>> UpdateRoleAsync(UpdateRoleCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RoleDetailDto>();

        try
        {
            var role = await _roleRepo.Query()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Role not found.";
                return response;
            }

            if (role.IsSystemRole)
            {
                if (role.Name != request.Name)
                {
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = "System roles cannot be renamed.";
                    return response;
                }
                if (!request.IsActive)
                {
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = "System roles cannot be deactivated.";
                    return response;
                }
            }

            // Check unique name if updated
            if (role.Name != request.Name && await _roleRepo.ExistsAsync(r => r.Name == request.Name))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "A role with this name already exists.";
                return response;
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.IsActive = request.IsActive;

            await _roleRepo.UpdateAsync(role);
            await _roleRepo.SaveAsync();

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

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Updated;
            response.Data = dto;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }

        return response;
    }

    // ─── Delete Role (Dynamic Roles) ─────────────────────────────────────────
    /// <summary>
    /// Deletes a custom role. System roles and roles currently assigned to users cannot be deleted.
    /// </summary>
    public async Task<Response<bool>> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();

        try
        {
            var role = await _roleRepo.GetByIdAsync(roleId);
            if (role is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Role not found.";
                return response;
            }

            if (role.IsSystemRole)
            {
                response.StatusCode = StatusCodes.Status403Forbidden;
                response.Message = "System roles cannot be deleted.";
                return response;
            }

            bool isAssignedToUser = await _userRepo.ExistsAsync(u => u.RoleId == roleId);
            if (isAssignedToUser)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Role is currently assigned to one or more users and cannot be deleted.";
                return response;
            }

            // Remove associated role permissions first
            await _rolePermissionRepo.Query()
                .Where(rp => rp.RoleId == roleId)
                .ExecuteDeleteAsync(cancellationToken);

            await _roleRepo.RemoveAsync(role);
            await _roleRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.Deleted;
            response.Data = true;
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
    /// System roles (SuperAdmin) cannot have permissions modified.
    /// At least one permission must remain after the update.
    /// </summary>
    public async Task<Response<object?>> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();

        var role = await _roleRepo.GetByIdAsync(request.RoleId);
        if (role is null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Role not found." };

        if (role.IsSystemRole && role.Name == RoleConstants.SuperAdmin)
        {
            return new Response<object?>
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Super Admin permissions are managed by the system and cannot be modified."
            };
        }

        try
        {
            // Enforce: cannot leave a role with zero permissions
            if (request.PermissionIds == null || !request.PermissionIds.Any())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.RoleRequiresPermissions;
                return response;
            }

            // 1. Execute immediate SQL delete to clear existing permission mappings
            await _rolePermissionRepo.Query()
                .Where(rp => rp.RoleId == request.RoleId)
                .ExecuteDeleteAsync(cancellationToken);

            // 2. Resolve distinct valid permission IDs
            var distinctInputIds = request.PermissionIds.Where(id => id != Guid.Empty).Distinct().ToList();
            var validPermissionIds = await _permissionRepo.Query()
                .Where(p => distinctInputIds.Contains(p.Id))
                .Select(p => p.Id)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!validPermissionIds.Any())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.RoleRequiresPermissions;
                return response;
            }

            // 3. Insert new unique RolePermission records
            var newRolePermissions = validPermissionIds.Select(permId => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = request.RoleId,
                PermissionId = permId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _rolePermissionRepo.AddRangeAsync(newRolePermissions);
            await _rolePermissionRepo.SaveAsync();

            return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Role permissions updated successfully." };
        }
        catch (Exception)
        {
            return new Response<object?> { StatusCode = StatusCodes.Status500InternalServerError, Message = Constants.Messages.ServerError };
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
