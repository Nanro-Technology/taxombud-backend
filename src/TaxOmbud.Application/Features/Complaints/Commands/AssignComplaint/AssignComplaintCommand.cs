using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Commands.AssignComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AssignComplaintCommand(Guid ComplaintId, Guid OfficerId, Guid AssignedByUserId)
    : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class AssignComplaintCommandValidator : AbstractValidator<AssignComplaintCommand>
{
    public AssignComplaintCommandValidator()
    {
        RuleFor(x => x.OfficerId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AssignComplaintCommandHandler : IRequestHandler<AssignComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public AssignComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(AssignComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        var officerExists = await _context.OfficerProfiles
            .AnyAsync(o => o.Id == request.OfficerId, cancellationToken);

        if (!officerExists)
            return Result<object?>.NotFound($"Officer '{request.OfficerId}' was not found.");

        complaint.Assign(request.OfficerId, request.AssignedByUserId);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
