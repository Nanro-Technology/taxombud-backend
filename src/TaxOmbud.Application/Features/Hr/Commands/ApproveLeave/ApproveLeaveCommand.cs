using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Hr.Commands.ApproveLeave;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ApproveLeaveCommand(Guid Id, bool Approved, string? SupervisorNote) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ApproveLeaveCommandValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SupervisorNote).MaximumLength(1000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ApproveLeaveCommandHandler : IRequestHandler<ApproveLeaveCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ApproveLeaveCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (leave == null)
            return Result<Unit>.NotFound("Leave request not found.");

        leave.Status = request.Approved ? "approved" : "rejected";
        leave.ApproverUserId = _currentUser.UserId ?? Guid.Empty;
        leave.SupervisorNote = request.SupervisorNote;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
