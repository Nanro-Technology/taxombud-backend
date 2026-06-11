using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetDashboard;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDashboardQuery() : IRequest<Result<DashboardStatsDto>>;

public record DashboardStatsDto(
    ComplaintsStatsDto Complaints,
    CasesStatsDto Cases,
    AppealsStatsDto Appeals,
    StaffStatsDto Staff,
    double AvgResolutionDays
);

public record ComplaintsStatsDto(int Total, int Open, int Closed);
public record CasesStatsDto(int Total, int Open, int Closed);
public record AppealsStatsDto(int Total, int Pending);
public record StaffStatsDto(int Officers, int Taxpayers);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalComplaints = await _context.Complaints.CountAsync(cancellationToken);
        var openComplaints = await _context.Complaints.CountAsync(c =>
            c.Status != Domain.Enums.ComplaintStatus.Closed &&
            c.Status != Domain.Enums.ComplaintStatus.Withdrawn, cancellationToken);
        var closedComplaints = await _context.Complaints.CountAsync(c =>
            c.Status == Domain.Enums.ComplaintStatus.Closed, cancellationToken);

        var totalCases = await _context.Cases.CountAsync(cancellationToken);
        var openCases = await _context.Cases.CountAsync(c =>
            c.Status != Domain.Enums.CaseStatus.Closed, cancellationToken);
        var closedCases = await _context.Cases.CountAsync(c =>
            c.Status == Domain.Enums.CaseStatus.Closed, cancellationToken);

        var totalAppeals = await _context.Appeals.CountAsync(cancellationToken);
        var pendingAppeals = await _context.Appeals.CountAsync(a =>
            a.Status == Domain.Enums.AppealStatus.Submitted ||
            a.Status == Domain.Enums.AppealStatus.UnderReview, cancellationToken);

        var totalOfficers = await _context.OfficerProfiles.CountAsync(cancellationToken);
        var totalTaxpayers = await _context.TaxpayerProfiles.CountAsync(cancellationToken);

        // Average days to close (based on complaints)
        var closedWithDates = await _context.Complaints
            .Where(c => c.Status == Domain.Enums.ComplaintStatus.Closed && c.ClosedAt != null)
            .Select(c => new { c.CreatedAt, c.ClosedAt })
            .ToListAsync(cancellationToken);

        double avgResolutionDays = closedWithDates.Count > 0
            ? closedWithDates.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays)
            : 0;

        var dto = new DashboardStatsDto(
            new ComplaintsStatsDto(totalComplaints, openComplaints, closedComplaints),
            new CasesStatsDto(totalCases, openCases, closedCases),
            new AppealsStatsDto(totalAppeals, pendingAppeals),
            new StaffStatsDto(totalOfficers, totalTaxpayers),
            Math.Round(avgResolutionDays, 1)
        );

        return Result<DashboardStatsDto>.Success(dto);
    }
}
