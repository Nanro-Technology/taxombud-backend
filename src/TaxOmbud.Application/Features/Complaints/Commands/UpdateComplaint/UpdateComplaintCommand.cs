using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Commands.UpdateComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateComplaintCommand(
    Guid Id,
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Priority
) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class UpdateComplaintCommandValidator : AbstractValidator<UpdateComplaintCommand>
{
    public UpdateComplaintCommandValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.TaxType).NotEmpty();
        RuleFor(x => x.TaxPeriod).NotEmpty();
        RuleFor(x => x.ComplaintCategory).NotEmpty();
        RuleFor(x => x.Priority).Must(p => p is "low" or "medium" or "high" or "urgent")
            .WithMessage("Priority must be low, medium, high, or urgent.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateComplaintCommandHandler : IRequestHandler<UpdateComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public UpdateComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(UpdateComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.Id}' was not found.");

        if (complaint.Status != TaxOmbud.Domain.Enums.ComplaintStatus.Draft)
            return Result<object?>.Failure("Only draft complaints can be edited.");

        // Use reflection-friendly setters via domain mutators
        complaint.UpdatePriority(request.Priority);
        // Direct property updates via a domain method we add
        complaint.UpdateDetails(request.Subject, request.Description, request.TaxType,
            request.TaxPeriod, request.ComplaintCategory, request.TaxOfficeRef, request.TinNumber);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<object?>.Success(null);
    }
}
