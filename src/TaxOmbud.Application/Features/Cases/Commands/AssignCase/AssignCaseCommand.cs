using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Application.Features.Cases.Commands.AssignCase;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AssignCaseCommand(Guid CaseId, Guid OfficerId) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class AssignCaseCommandValidator : AbstractValidator<AssignCaseCommand>
{
    public AssignCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.OfficerId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AssignCaseCommandHandler : IRequestHandler<AssignCaseCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AssignCaseCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(AssignCaseCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (complaint == null)
            return Result<Unit>.NotFound("Complaint not found.");

        var officer = await _context.OfficerProfiles.FirstOrDefaultAsync(o => o.Id == request.OfficerId, cancellationToken);
        if (officer == null)
            return Result<Unit>.Failure("Officer profile not found.");

        var actorUserId = _currentUser.UserId ?? Guid.Empty;
        complaint.Assign(request.OfficerId, actorUserId);

        var kase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == request.CaseId, cancellationToken);
        if (kase == null)
        {
            kase = new Case(complaint.Id, complaint.Subject, Guid.NewGuid(), complaint.Priority);
            kase.Open(ReferenceNumber.From(ReferenceNumber.Generate("CAS")));
            _context.Cases.Add(kase);
        }
        kase.Assign(request.OfficerId, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
