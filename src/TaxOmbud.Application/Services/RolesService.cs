using TaxOmbud.Common.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Roles.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class RolesService : IRolesService
{
    private readonly IApplicationDbContext _context;

    public RolesService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<CreateRoleResponse>> CreateRoleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateRoleResponse>();

        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
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
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

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
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Role not found." };

        try
        {
            // Remove current permission assignments
            _context.RolePermissions.RemoveRange(role.RolePermissions);

            // Add new permission assignments by Permission Guid ID
            foreach (var permId in request.PermissionIds)
            {
                var permission = await _context.Permissions.FindAsync(new object[] { permId }, cancellationToken);
                if (permission == null)
                    return new Response<object?> { StatusCode = StatusCodes.Status400BadRequest, Message = $"Permission with ID '{permId}' does not exist." };

                role.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = request.RoleId,
                    PermissionId = permId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
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
            var permissions = await _context.Permissions
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
        var role = await _context.Roles
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
            var roles = await _context.Roles
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