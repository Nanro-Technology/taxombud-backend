using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Roles.Commands.UpdateRolePermissions;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateRolePermissionsCommand(Guid RoleId, string[] PermissionCodes) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionCodes).NotNull();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateRolePermissionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            return Result<Unit>.NotFound("Role not found.");

        // Remove current permissions
        _context.RolePermissions.RemoveRange(role.RolePermissions);

        // Add new permissions
        foreach (var permCode in request.PermissionCodes)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == permCode, cancellationToken);
            if (permission == null)
                return Result<Unit>.Failure($"Permission with code '{permCode}' does not exist.");

            role.RolePermissions.Add(new RolePermission
            {
                RoleId = request.RoleId,
                PermissionCode = permCode
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
