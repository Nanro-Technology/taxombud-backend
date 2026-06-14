using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetSlaReports;

public class GetSlaReportsQuery : ReportFilterDto, IRequest<SlaReportDto> { }

public class GetSlaReportsQueryHandler : IRequestHandler<GetSlaReportsQuery, SlaReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetSlaReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlaReportDto> Handle(GetSlaReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cases.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(c => c.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(c => c.CreatedAt <= request.EndDate.Value);

        // A case is breached if ResolvedAt is > DueDate, or if it is still open and DueDate < now
        var now = DateTimeOffset.UtcNow;
        var totalCases = await query.CountAsync(cancellationToken);
        
        var breachedCases = await query.CountAsync(c => 
            (c.ClosedAt != null && c.DueDate != null && c.ClosedAt > c.DueDate) || 
            (c.ClosedAt == null && c.DueDate != null && c.DueDate < now), cancellationToken);

        var withinSla = totalCases - breachedCases;
        var compliance = totalCases > 0 ? ((double)withinSla / totalCases) * 100 : 0;

        var resolvedCases = await query
            .Where(c => c.Status == TaxOmbud.Domain.Enums.CaseStatus.Closed && c.ClosedAt != null)
            .Select(c => new { c.CreatedAt, c.ClosedAt })
            .ToListAsync(cancellationToken);

        double avgResolutionTime = 0;
        if (resolvedCases.Any())
        {
            avgResolutionTime = resolvedCases.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalHours);
        }

        return new SlaReportDto
        {
            TotalCasesMeasured = totalCases,
            CasesWithinSla = withinSla,
            CasesBreachedSla = breachedCases,
            SlaCompliancePercentage = compliance,
            AverageResolutionTimeHours = avgResolutionTime
        };
    }
}
