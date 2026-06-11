using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Roles.Commands.CreateRole;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateRoleCommand(string Name, string Code, string Scope, string? Description) : IRequest<Result<CreateRoleResponse>>;

public record CreateRoleResponse(Guid Id, string Name, string Code, string Scope, string? Description);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Scope).Must(s => s.Equals("sitewide", StringComparison.OrdinalIgnoreCase) || s.Equals("private", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Scope must be 'sitewide' or 'private'.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateRoleResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var codeNormalized = request.Code.Trim().ToLowerInvariant();
        if (await _context.Roles.AnyAsync(r => r.Code == codeNormalized, cancellationToken))
            return Result<CreateRoleResponse>.Failure("Role code already exists.");

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

        return Result<CreateRoleResponse>.Success(new CreateRoleResponse(role.Id, role.Name, role.Code, role.Scope, role.Description));
    }
}
