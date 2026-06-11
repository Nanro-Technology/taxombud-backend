using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.ApproveClosure;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ApproveClosureCommand(Guid CaseId, bool Approve, string Rationale) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ApproveClosureCommandValidator : AbstractValidator<ApproveClosureCommand>
{
    public ApproveClosureCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Rationale).NotEmpty().MinimumLength(100)
            .WithMessage("Terminal CE approval requires a written rationale of at least 100 characters.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ApproveClosureCommandHandler : IRequestHandler<ApproveClosureCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ApproveClosureCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(ApproveClosureCommand request, CancellationToken cancellationToken)
    {
        var kase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == request.CaseId, cancellationToken);
        if (kase == null)
            return Result<Unit>.NotFound("Case not found.");

        var actorUserId = _currentUser.UserId ?? Guid.Empty;

        if (request.Approve)
        {
            kase.Close("Resolved", request.Rationale, actorUserId);
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
            if (complaint != null)
            {
                complaint.Close(request.Rationale, actorUserId);
            }
        }
        else
        {
            // Reject and return case to B3 Stage
            kase.UpdateStatus(CaseStatus.InProgress, "b3", actorUserId);
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
            if (complaint != null)
            {
                complaint.UpdateStage("b3");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
