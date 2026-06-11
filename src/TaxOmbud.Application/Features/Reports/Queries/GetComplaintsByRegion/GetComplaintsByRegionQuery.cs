using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Queries.GetComplaintsByRegion;

public record GetComplaintsByRegionQuery() : IRequest<Result<List<RegionReportDto>>>;

public record RegionReportDto(string Region, int Count);

public class GetComplaintsByRegionQueryHandler : IRequestHandler<GetComplaintsByRegionQuery, Result<List<RegionReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsByRegionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RegionReportDto>>> Handle(GetComplaintsByRegionQuery request, CancellationToken cancellationToken)
    {
        // For simplicity, we assume Region is part of the TaxpayerProfile or mapped from address
        // Since we don't have a direct Region field on Complaint, we might group by Taxpayer's state/region
        var stats = await _context.Complaints
            .Include(c => c.Taxpayer)
            .Where(c => c.Taxpayer != null && !string.IsNullOrEmpty(c.Taxpayer.City))
            .GroupBy(c => c.Taxpayer.City)
            .Select(g => new RegionReportDto(g.Key ?? "Unknown", g.Count()))
            .ToListAsync(cancellationToken);

        return Result<List<RegionReportDto>>.Success(stats);
    }
}
