using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaintById;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintByIdQuery(Guid Id) : IRequest<Result<ComplaintDetailDto>>;

public record ComplaintDetailDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Status,
    string CurrentStage,
    string Priority,
    bool RequiresApprovalToClose,
    DateTimeOffset? ClosedAt,
    string? ClosureReason,
    string? WithdrawalReason,
    TaxpayerSummary Taxpayer,
    OfficerSummary? AssignedOfficer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record TaxpayerSummary(Guid Id, string FullName, string? Email, string? Phone);
public record OfficerSummary(Guid Id, string FullName, string? Email);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintByIdQueryHandler : IRequestHandler<GetComplaintByIdQuery, Result<ComplaintDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ComplaintDetailDto>> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .AsNoTracking()
            .Include(c => c.Taxpayer)
            .Include(c => c.AssignedOfficer)
                .ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (complaint is null)
            return Result<ComplaintDetailDto>.NotFound($"Complaint '{request.Id}' was not found.");

        var taxpayer = complaint.Taxpayer;
        var officer = complaint.AssignedOfficer;

        var dto = new ComplaintDetailDto(
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
                taxpayer?.Phone
            ),
            AssignedOfficer: officer != null && officer.User != null
                ? new OfficerSummary(officer.Id, officer.User.FullName, officer.User.Email)
                : null,
            CreatedAt: complaint.CreatedAt,
            UpdatedAt: complaint.UpdatedAt
        );

        return Result<ComplaintDetailDto>.Success(dto);
    }
}
