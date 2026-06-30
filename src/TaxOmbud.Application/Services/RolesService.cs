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
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;
using FluentValidation;

namespace TaxOmbud.Application.Services;

public class RolesService : IRolesService
{
    private readonly IApplicationDbContext _context;

    public RolesService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<CreateRoleResponse>> CreateRoleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<CreateRoleResponse>();
        var codeNormalized = request.Code.Trim().ToLowerInvariant();
        if (await _context.Roles.AnyAsync(r => r.Code == codeNormalized, cancellationToken))
            return new Response<CreateRoleResponse> { StatusCode = StatusCodes.Status400BadRequest, Message = "Role code already exists." };
        try
        {

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = codeNormalized,
            Scope = request.Scope.ToLowerInvariant() == "private" ? "private" : "sitewide",
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return new Response<CreateRoleResponse> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = new CreateRoleResponse(role.Id, role.Name, role.Code, role.Scope, role.Description) };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

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

        // Remove current permissions
        _context.RolePermissions.RemoveRange(role.RolePermissions);

        // Add new permissions
        foreach (var permCode in request.PermissionCodes)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == permCode, cancellationToken);
            if (permission == null)
                return new Response<object?> { StatusCode = StatusCodes.Status400BadRequest, Message = $"Permission with code '{permCode}' does not exist." };

            role.RolePermissions.Add(new RolePermission
            {
                RoleId = request.RoleId,
                PermissionCode = permCode
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<IEnumerable<PermissionDetailDto>>> GetPermissionsAsync(GetPermissionsQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<IEnumerable<PermissionDetailDto>>();
        try
        {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Entity)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionDetailDto(p.Code, p.Action, p.Entity, p.Description))
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
        }}

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
            role.Code,
            role.Scope,
            role.Description,
            role.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission!.Code,
                rp.Permission.Action,
                rp.Permission.Entity,
                rp.Permission.Description
            ))
        );

        return new Response<RoleDetailDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = dto };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<IEnumerable<RoleDto>>> GetRolesAsync(GetRolesQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<IEnumerable<RoleDto>>();
        try
        {
        var roles = await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(r.Id, r.Name, r.Code, r.Scope, r.Description))
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
        }}

}