using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Appeals.Commands.ReviewAppeal;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ReviewAppealCommand(Guid AppealId, string Action, string Notes) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ReviewAppealCommandValidator : AbstractValidator<ReviewAppealCommand>
{
    public ReviewAppealCommandValidator()
    {
        RuleFor(x => x.AppealId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(2000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ReviewAppealCommandHandler : IRequestHandler<ReviewAppealCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ReviewAppealCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(ReviewAppealCommand request, CancellationToken cancellationToken)
    {
        var appeal = await _context.Appeals.FirstOrDefaultAsync(a => a.Id == request.AppealId, cancellationToken);
        if (appeal == null)
            return Result<Unit>.NotFound("Appeal not found.");

        var actorUserId = _currentUser.UserId ?? Guid.Empty;

        if (request.Action.ToLowerInvariant() == "uphold")
        {
            appeal.Uphold(actorUserId, request.Notes);
        }
        else if (request.Action.ToLowerInvariant() == "dismiss")
        {
            appeal.Dismiss(actorUserId, request.Notes);
        }
        else
        {
            appeal.Review(actorUserId, request.Notes);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
