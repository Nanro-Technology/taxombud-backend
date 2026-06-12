using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetCaseReports;

public class GetCaseReportsQuery : ReportFilterDto, IRequest<CaseReportDto> { }

public class GetCaseReportsQueryHandler : IRequestHandler<GetCaseReportsQuery, CaseReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetCaseReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CaseReportDto> Handle(GetCaseReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cases.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(c => c.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(c => c.CreatedAt <= request.EndDate.Value);

        var totalCases = await query.CountAsync(cancellationToken);
        
        var statuses = await query.GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var priorities = await query.GroupBy(c => c.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var departments = await query.GroupBy(c => c.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var openCases = statuses.Where(s => s.Status != TaxOmbud.Domain.Enums.CaseStatus.Closed).Sum(s => s.Count);
        var closedCases = statuses.Where(s => s.Status == TaxOmbud.Domain.Enums.CaseStatus.Closed).Sum(s => s.Count);

        // Aging
        var now = DateTimeOffset.UtcNow;
        var twoDaysAgo = now.AddDays(-2);
        var sevenDaysAgo = now.AddDays(-7);
        var fourteenDaysAgo = now.AddDays(-14);

        var openQuery = query.Where(c => c.Status != TaxOmbud.Domain.Enums.CaseStatus.Closed);
        var bucket1 = await openQuery.CountAsync(c => c.CreatedAt >= twoDaysAgo, cancellationToken);
        var bucket2 = await openQuery.CountAsync(c => c.CreatedAt < twoDaysAgo && c.CreatedAt >= sevenDaysAgo, cancellationToken);
        var bucket3 = await openQuery.CountAsync(c => c.CreatedAt < sevenDaysAgo && c.CreatedAt >= fourteenDaysAgo, cancellationToken);
        var bucket4 = await openQuery.CountAsync(c => c.CreatedAt < fourteenDaysAgo, cancellationToken);

        var dto = new CaseReportDto
        {
            TotalCases = totalCases,
            OpenCases = openCases,
            ClosedCases = closedCases,
            EscalatedCases = statuses.FirstOrDefault(s => s.Status == TaxOmbud.Domain.Enums.CaseStatus.UnderReview)?.Count ?? 0,
            CasesByStatus = statuses.ToDictionary(k => k.Status.ToString(), v => v.Count),
            CasesByPriority = priorities.ToDictionary(k => k.Priority ?? "Unknown", v => v.Count),
            CasesByCategory = departments.ToDictionary(k => k.DepartmentId?.ToString() ?? "None", v => v.Count),
            AgingBuckets = new Dictionary<string, int>
            {
                { "0-2 days", bucket1 },
                { "3-7 days", bucket2 },
                { "8-14 days", bucket3 },
                { "15+ days", bucket4 }
            }
        };

        return dto;
    }
}
