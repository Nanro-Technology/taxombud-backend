using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Commands.CloseComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CloseComplaintCommand(Guid ComplaintId, string Reason, Guid ClosedByUserId)
    : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class CloseComplaintCommandValidator : AbstractValidator<CloseComplaintCommand>
{
    public CloseComplaintCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CloseComplaintCommandHandler : IRequestHandler<CloseComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public CloseComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(CloseComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        try
        {
            complaint.Close(request.Reason, request.ClosedByUserId);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<object?>.Success(null);
        }
        catch (TaxOmbud.Domain.Exceptions.DomainException ex)
        {
            return Result<object?>.Failure(ex.Message);
        }
    }
}
