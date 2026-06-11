using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Users.Commands.UpdateCurrentUser;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateCurrentUserCommand(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle
) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateCurrentUserCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<Unit>.Failure("User is not authenticated.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user is null)
            return Result<Unit>.NotFound("User not found.");

        user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.JobTitle);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
