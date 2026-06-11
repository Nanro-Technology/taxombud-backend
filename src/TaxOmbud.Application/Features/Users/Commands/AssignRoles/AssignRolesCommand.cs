using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Users.Commands.AssignRoles;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AssignRolesCommand(Guid Id, Guid[] RoleIds) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoleIds).NotNull();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public AssignRolesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return Result<Unit>.NotFound("User not found.");

        // Remove old roles
        _context.UserRoles.RemoveRange(user.UserRoles);

        // Add new roles
        foreach (var roleId in request.RoleIds)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
            if (!roleExists)
                return Result<Unit>.Failure($"Role with ID '{roleId}' does not exist.");

            user.AddRole(roleId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
