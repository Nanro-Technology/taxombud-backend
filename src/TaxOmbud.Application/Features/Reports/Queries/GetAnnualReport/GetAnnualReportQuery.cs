using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Reports.Queries.GetAnnualReport;

public record GetAnnualReportQuery(int Year) : IRequest<Result<AnnualReportDto>>;

public record AnnualReportDto(int Year, int TotalComplaints, int TotalCases, int ResolvedCases, double AverageResolutionDays);

public class GetAnnualReportQueryHandler : IRequestHandler<GetAnnualReportQuery, Result<AnnualReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAnnualReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AnnualReportDto>> Handle(GetAnnualReportQuery request, CancellationToken cancellationToken)
    {
        var totalComplaints = await _context.Complaints
            .Where(c => c.CreatedAt.Year == request.Year)
            .CountAsync(cancellationToken);

        var cases = await _context.Cases
            .Where(c => c.CreatedAt.Year == request.Year)
            .ToListAsync(cancellationToken);

        var totalCases = cases.Count;
        var resolvedCasesList = cases.Where(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue).ToList();
        var resolvedCases = resolvedCasesList.Count;

        double avgDays = 0;
        if (resolvedCasesList.Any())
        {
            avgDays = resolvedCasesList.Average(c => (c.ClosedAt!.Value - c.CreatedAt).TotalDays);
        }

        return Result<AnnualReportDto>.Success(new AnnualReportDto(request.Year, totalComplaints, totalCases, resolvedCases, avgDays));
    }
}
