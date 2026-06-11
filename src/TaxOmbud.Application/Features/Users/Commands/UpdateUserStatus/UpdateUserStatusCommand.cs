using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Users.Commands.UpdateUserStatus;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateUserStatusCommand(Guid Id, bool Activate) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null)
            return Result<Unit>.NotFound("User not found.");

        if (request.Activate)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
