using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Complaints.Commands.UpdateComplaintStatus;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateComplaintStatusCommand(
    Guid ComplaintId,
    ComplaintStatus Status,
    string? Reason
) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class UpdateComplaintStatusCommandValidator : AbstractValidator<UpdateComplaintStatusCommand>
{
    public UpdateComplaintStatusCommandValidator()
    {
        RuleFor(x => x.ComplaintId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateComplaintStatusCommandHandler : IRequestHandler<UpdateComplaintStatusCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateComplaintStatusCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<object?>> Handle(UpdateComplaintStatusCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        var userId = _currentUser.UserId ?? Guid.Empty;

        try
        {
            switch (request.Status)
            {
                case ComplaintStatus.Submitted:
                    complaint.Submit();
                    break;
                case ComplaintStatus.UnderReview:
                    if (complaint.Status == ComplaintStatus.Closed)
                    {
                        complaint.Reopen(userId);
                    }
                    else
                    {
                        return Result<object?>.Failure("Direct transition to UnderReview is only allowed from Closed status by reopening. Otherwise, assign an officer to start review.");
                    }
                    break;
                case ComplaintStatus.Escalated:
                    complaint.Escalate(request.Reason ?? "Status updated to Escalated.", userId);
                    break;
                case ComplaintStatus.Resolved:
                    complaint.Resolve(userId);
                    break;
                case ComplaintStatus.Closed:
                    complaint.Close(request.Reason ?? "Status updated to Closed.", userId);
                    break;
                case ComplaintStatus.Withdrawn:
                    complaint.Withdraw(request.Reason ?? "Status updated to Withdrawn.", userId);
                    break;
                default:
                    return Result<object?>.Failure($"Invalid or unsupported status transition to '{request.Status}'.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<object?>.Success(null);
        }
        catch (DomainException ex)
        {
            return Result<object?>.Failure(ex.Message);
        }
    }
}
