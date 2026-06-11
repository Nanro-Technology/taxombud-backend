using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaintById;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaintByReference;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintByReferenceQuery(string ReferenceNumber) : IRequest<Result<ComplaintDetailDto>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintByReferenceQueryHandler
    : IRequestHandler<GetComplaintByReferenceQuery, Result<ComplaintDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintByReferenceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ComplaintDetailDto>> Handle(
        GetComplaintByReferenceQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .AsNoTracking()
            .Include(c => c.Taxpayer)
            .Include(c => c.AssignedOfficer)
                .ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(
                c => c.ReferenceNumber == request.ReferenceNumber.Trim().ToUpperInvariant(),
                cancellationToken);

        if (complaint is null)
            return Result<ComplaintDetailDto>.NotFound(
                $"Complaint with reference '{request.ReferenceNumber}' was not found.");

        var taxpayer = complaint.Taxpayer;
        var officer  = complaint.AssignedOfficer;

        return Result<ComplaintDetailDto>.Success(new ComplaintDetailDto(
            Id: complaint.Id,
            ReferenceNumber: complaint.ReferenceNumber,
            Subject: complaint.Subject,
            Description: complaint.Description,
            TaxType: complaint.TaxType,
            TaxPeriod: complaint.TaxPeriod,
            ComplaintCategory: complaint.ComplaintCategory,
            TaxOfficeRef: complaint.TaxOfficeRef,
            TinNumber: complaint.TinNumber,
            Status: complaint.Status.ToString(),
            CurrentStage: complaint.CurrentStage,
            Priority: complaint.Priority,
            RequiresApprovalToClose: complaint.RequiresApprovalToClose,
            ClosedAt: complaint.ClosedAt,
            ClosureReason: complaint.ClosureReason,
            WithdrawalReason: complaint.WithdrawalReason,
            Taxpayer: new TaxpayerSummary(
                taxpayer?.Id ?? Guid.Empty,
                taxpayer != null ? $"{taxpayer.FirstName} {taxpayer.LastName}" : "Unknown",
                taxpayer?.Email.Value,
                taxpayer?.Phone),
            AssignedOfficer: officer?.User != null
                ? new OfficerSummary(officer.Id, officer.User.FullName, officer.User.Email)
                : null,
            CreatedAt: complaint.CreatedAt,
            UpdatedAt: complaint.UpdatedAt
        ));
    }
}
