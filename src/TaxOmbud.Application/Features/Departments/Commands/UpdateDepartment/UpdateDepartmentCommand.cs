using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Departments.Commands.UpdateDepartment;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateDepartmentCommand(Guid Id, string Name, string RoutingMode, string? Description, Guid? HeadUserId) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoutingMode).Must(m => m.Equals("head", StringComparison.OrdinalIgnoreCase) || m.Equals("members", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Routing mode must be 'head' or 'members'.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (department == null)
            return Result<Unit>.NotFound("Department not found.");

        if (request.HeadUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.HeadUserId.Value, cancellationToken);
            if (!userExists)
                return Result<Unit>.Failure("Head user not found.");
            
            department.HeadUserId = request.HeadUserId.Value;
        }
        else
        {
            department.HeadUserId = null;
        }

        department.Name = request.Name;
        department.RoutingMode = request.RoutingMode.ToLowerInvariant() == "head" ? "head" : "members";
        department.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
