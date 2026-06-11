using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Users.Commands.ApplyPermissionOverrides;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ApplyPermissionOverridesCommand(Guid Id, PermissionOverrideDto[] Overrides) : IRequest<Result<Unit>>;

public record PermissionOverrideDto(string PermissionCode, string Mode);

// ─── Validator ────────────────────────────────────────────────────────────────

public class ApplyPermissionOverridesCommandValidator : AbstractValidator<ApplyPermissionOverridesCommand>
{
    public ApplyPermissionOverridesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Overrides).NotNull();
        RuleForEach(x => x.Overrides).ChildRules(over =>
        {
            over.RuleFor(x => x.PermissionCode).NotEmpty();
            over.RuleFor(x => x.Mode).Must(m => m.Equals("grant", StringComparison.OrdinalIgnoreCase) || m.Equals("deny", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Mode must be 'grant' or 'deny'.");
        });
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ApplyPermissionOverridesCommandHandler : IRequestHandler<ApplyPermissionOverridesCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ApplyPermissionOverridesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ApplyPermissionOverridesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissionOverrides)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return Result<Unit>.NotFound("User not found.");

        // Remove existing overrides
        _context.UserPermissionOverrides.RemoveRange(user.UserPermissionOverrides);

        // Add new overrides
        foreach (var over in request.Overrides)
        {
            var permExists = await _context.Permissions.AnyAsync(p => p.Code == over.PermissionCode, cancellationToken);
            if (!permExists)
                return Result<Unit>.Failure($"Permission with Code '{over.PermissionCode}' does not exist.");

            user.UserPermissionOverrides.Add(new UserPermissionOverride
            {
                UserId = request.Id,
                PermissionCode = over.PermissionCode,
                Mode = over.Mode.ToLowerInvariant() == "deny" ? "deny" : "grant"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
