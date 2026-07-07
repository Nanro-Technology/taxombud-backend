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

    public async Task<Response<CreateRoleResponse>> CreateRoleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateRoleResponse>();

        if (await _roleRepo.ExistsAsync(r => r.Name == request.Name))
            return new Response<CreateRoleResponse> { StatusCode = StatusCodes.Status400BadRequest, Message = "A role with this name already exists." };

        try
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsSystemRole = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveAsync();

            return new Response<CreateRoleResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Role created successfully.",
                Data = new CreateRoleResponse(role.Id, role.Name, role.Description)
            };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<object?>> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();

        var role = await _roleRepo.Query()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Role not found." };

        try
        {
            await _rolePermissionRepo.RemoveRangeAsync(role.RolePermissions);

            foreach (var permId in request.PermissionIds)
            {
                var permission = await _permissionRepo.GetByIdAsync(permId);
                if (permission == null)
                    return new Response<object?> { StatusCode = StatusCodes.Status400BadRequest, Message = $"Permission with ID '{permId}' does not exist." };

                await _rolePermissionRepo.AddAsync(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = request.RoleId,
                    PermissionId = permId,
                    CreatedAt = DateTime.UtcNow
                });
            }

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
            response.Message = "Success";
            response.Data = permissions;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<RoleDetailDto>> GetRoleByIdAsync(GetRoleByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<RoleDetailDto>();

        var role = await _roleRepo.Query()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
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

            return new Response<RoleDetailDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = dto };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

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
            response.Message = "Success";
            response.Data = roles;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }
}
