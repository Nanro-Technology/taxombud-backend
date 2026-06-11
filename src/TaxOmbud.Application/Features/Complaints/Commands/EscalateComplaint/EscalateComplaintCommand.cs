using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Commands.EscalateComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record EscalateComplaintCommand(Guid ComplaintId, string Reason, Guid EscalatedByUserId)
    : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class EscalateComplaintCommandValidator : AbstractValidator<EscalateComplaintCommand>
{
    public EscalateComplaintCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class EscalateComplaintCommandHandler : IRequestHandler<EscalateComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public EscalateComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(EscalateComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        try
        {
            complaint.Escalate(request.Reason, request.EscalatedByUserId);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<object?>.Success(null);
        }
        catch (TaxOmbud.Domain.Exceptions.DomainException ex)
        {
            return Result<object?>.Failure(ex.Message);
        }
    }
}
