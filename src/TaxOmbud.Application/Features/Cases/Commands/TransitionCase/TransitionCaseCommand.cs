using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.TransitionCase;

// ─── Command ─────────────────────────────────────────────────────────────────

public record TransitionCaseCommand(Guid CaseId, string TargetStage, string? Reason) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class TransitionCaseCommandValidator : AbstractValidator<TransitionCaseCommand>
{
    public TransitionCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.TargetStage).NotEmpty().MaximumLength(50);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class TransitionCaseCommandHandler : IRequestHandler<TransitionCaseCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public TransitionCaseCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(TransitionCaseCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (complaint == null)
            return Result<Unit>.NotFound("Complaint/Case not found.");

        var targetStage = request.TargetStage.ToLowerInvariant();
        var actorUserId = _currentUser.UserId ?? Guid.Empty;

        complaint.UpdateStage(targetStage);

        var kase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == request.CaseId, cancellationToken);
        if (kase != null)
        {
            if (targetStage == "closed")
            {
                var reason = request.Reason ?? "Closed by system transition";
                kase.Close(reason, "Transitioned to closed.", actorUserId);
                complaint.Close(reason, actorUserId);
            }
            else
            {
                kase.UpdateStatus(CaseStatus.InProgress, targetStage, actorUserId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
