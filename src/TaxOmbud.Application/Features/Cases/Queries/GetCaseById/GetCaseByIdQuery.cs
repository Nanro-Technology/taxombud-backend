using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Cases.Queries.GetCaseById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetCaseByIdQuery(Guid Id) : IRequest<Result<CaseDetailDto>>;

public record CaseDetailDto(
    Guid Id,
    string CaseNumber,
    string Subject,
    string? Summary,
    string Priority,
    string Status,
    string CurrentStage,
    CaseOfficerDto? AssignedOfficer,
    CaseDepartmentDto? Department,
    DateTimeOffset? DueDate,
    DateTimeOffset? ClosedAt,
    string? Outcome,
    string? FindingsSummary,
    CaseComplaintDto Complaint,
    IEnumerable<FindingDto> Findings,
    IEnumerable<RecommendationDto> Recommendations,
    IEnumerable<MilestoneDto> Milestones,
    IEnumerable<StatusHistoryDto> StatusHistory
);

public record CaseOfficerDto(Guid Id, string FullName, string Email);
public record CaseDepartmentDto(Guid Id, string Name);
public record CaseComplaintDto(Guid Id, string ReferenceNumber, ComplaintTaxpayerDto? Taxpayer, string TaxType, string TaxPeriod, string ComplaintCategory);
public record ComplaintTaxpayerDto(Guid Id, string FullName, string Email);
public record FindingDto(Guid Id, string Description, DateTimeOffset CreatedAt);
public record RecommendationDto(Guid Id, string RecommendationText, Guid ApprovedByUserId, DateTimeOffset CreatedAt);
public record MilestoneDto(Guid Id, string Title, string? Description, DateTimeOffset CreatedAt);
public record StatusHistoryDto(Guid Id, string PreviousStatus, string NewStatus, Guid ChangedByUserId, DateTimeOffset CreatedAt);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetCaseByIdQueryHandler : IRequestHandler<GetCaseByIdQuery, Result<CaseDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CaseDetailDto>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Cases
            .Include(x => x.Complaint)
                .ThenInclude(co => co.Taxpayer!)
            .Include(x => x.AssignedOfficer!)
                .ThenInclude(o => o.User)
            .Include(x => x.Department)
            .Include(x => x.Findings)
            .Include(x => x.Recommendations)
            .Include(x => x.Milestones)
            .Include(x => x.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (c == null)
            return Result<CaseDetailDto>.NotFound("Case not found.");

        var dto = new CaseDetailDto(
            c.Id,
            c.CaseNumber.Value,
            c.Subject,
            c.Summary,
            c.Priority,
            c.Status.ToString(),
            c.CurrentStage,
            c.AssignedOfficer != null && c.AssignedOfficer.User != null ? new CaseOfficerDto(c.AssignedOfficer.Id, c.AssignedOfficer.User.FullName, c.AssignedOfficer.User.Email) : null,
            c.Department != null ? new CaseDepartmentDto(c.Department.Id, c.Department.Name) : null,
            c.DueDate,
            c.ClosedAt,
            c.Outcome,
            c.FindingsSummary,
            new CaseComplaintDto(
                c.Complaint.Id,
                c.Complaint.ReferenceNumber,
                c.Complaint.Taxpayer != null ? new ComplaintTaxpayerDto(c.Complaint.Taxpayer.Id, c.Complaint.Taxpayer.FirstName + " " + c.Complaint.Taxpayer.LastName, c.Complaint.Taxpayer.Email.Value) : null,
                c.Complaint.TaxType,
                c.Complaint.TaxPeriod,
                c.Complaint.ComplaintCategory
            ),
            c.Findings.Select(f => new FindingDto(f.Id, f.Description, f.CreatedAt)),
            c.Recommendations.Select(r => new RecommendationDto(r.Id, r.RecommendationText, r.ApprovedByUserId ?? Guid.Empty, r.CreatedAt)),
            c.Milestones.Select(m => new MilestoneDto(m.Id, m.Title, m.Description, m.CreatedAt)),
            c.StatusHistory.Select(h => new StatusHistoryDto(h.Id, h.OldStatus.ToString(), h.NewStatus.ToString(), h.ChangedByUserId, h.TransitionedAt))
        );

        return Result<CaseDetailDto>.Success(dto);
    }
}
