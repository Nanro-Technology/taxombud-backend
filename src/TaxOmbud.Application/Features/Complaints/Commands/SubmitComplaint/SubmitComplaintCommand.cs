using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Application.Features.Complaints.Commands.SubmitComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record SubmitComplaintCommand(
    Guid TaxpayerId,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string Subject,
    string Description,
    string? TaxOfficeRef,
    string? TinNumber
) : IRequest<Result<SubmitComplaintResponse>>;

public record SubmitComplaintResponse(Guid ComplaintId, string ReferenceNumber, string Status);

// ─── Validator ────────────────────────────────────────────────────────────────

public class SubmitComplaintCommandValidator : AbstractValidator<SubmitComplaintCommand>
{
    private static readonly string[] ValidTaxTypes =
        ["PIT", "CIT", "VAT", "CGT", "WHT", "EDT", "PAYE", "Stamp Duty", "Other"];

    private static readonly string[] ValidCategories =
        ["Refund", "Assessment", "Enforcement", "Objection", "Interpretation", "Other"];

    public SubmitComplaintCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
        RuleFor(x => x.TaxType).NotEmpty().Must(t => Array.Exists(ValidTaxTypes, v => v == t))
            .WithMessage("Invalid TaxType specified.");
        RuleFor(x => x.TaxPeriod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ComplaintCategory).NotEmpty().Must(c => Array.Exists(ValidCategories, v => v == c))
            .WithMessage("Invalid ComplaintCategory specified.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class SubmitComplaintCommandHandler : IRequestHandler<SubmitComplaintCommand, Result<SubmitComplaintResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public SubmitComplaintCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SubmitComplaintResponse>> Handle(SubmitComplaintCommand request, CancellationToken cancellationToken)
    {
        var refNumber = ReferenceNumber.Generate("CMP");

        var complaint = Complaint.Create(
            taxpayerId: request.TaxpayerId,
            taxType: request.TaxType,
            taxPeriod: request.TaxPeriod,
            category: request.ComplaintCategory,
            subject: request.Subject,
            description: request.Description,
            referenceNumber: refNumber,
            taxOfficeRef: request.TaxOfficeRef,
            tinNumber: request.TinNumber
        );

        complaint.Submit();

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SubmitComplaintResponse>.Success(new SubmitComplaintResponse(
            complaint.Id,
            complaint.ReferenceNumber,
            complaint.Status.ToString()
        ));
    }
}
