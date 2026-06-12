using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Crm.DTOs;

namespace TaxOmbud.Application.Features.Crm.Queries.GetCalls;

public record GetCallsQuery : IRequest<List<CallDto>>;

public class GetCallsQueryHandler : IRequestHandler<GetCallsQuery, List<CallDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCallsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CallDto>> Handle(GetCallsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Calls
            .AsNoTracking()
            .Select(x => new CallDto
            {
                Id = x.Id,
                Subject = x.Subject,
                CallerType = x.CallerType,
                CallerMethod = x.CallerMethod,
                CallerIdentifier = x.CallerIdentifier,
                CalleeMethod = x.CalleeMethod,
                CalleeIdentifier = x.CalleeIdentifier,
                Direction = x.Direction,
                Status = x.Status,
                Phone = x.Phone,
                Notes = x.Notes,
                LinkedToId = x.LinkedToId,
                AgentId = x.AgentId,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
