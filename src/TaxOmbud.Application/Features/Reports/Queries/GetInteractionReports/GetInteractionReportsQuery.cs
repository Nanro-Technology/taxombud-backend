using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Reports.DTOs;

namespace TaxOmbud.Application.Features.Reports.Queries.GetInteractionReports;

public class GetInteractionReportsQuery : ReportFilterDto, IRequest<InteractionReportDto> { }

public class GetInteractionReportsQueryHandler : IRequestHandler<GetInteractionReportsQuery, InteractionReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetInteractionReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InteractionReportDto> Handle(GetInteractionReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Interactions.AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(i => i.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(i => i.CreatedAt <= request.EndDate.Value);

        var total = await query.CountAsync(cancellationToken);
        
        var channels = await query.GroupBy(i => i.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var directions = await query.GroupBy(i => i.Direction)
            .Select(g => new { Direction = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new InteractionReportDto
        {
            TotalInteractions = total,
            InteractionsByChannel = channels.ToDictionary(k => k.Channel ?? "Unknown", v => v.Count),
            InteractionsByDirection = directions.ToDictionary(k => k.Direction ?? "Unknown", v => v.Count)
        };
    }
}
