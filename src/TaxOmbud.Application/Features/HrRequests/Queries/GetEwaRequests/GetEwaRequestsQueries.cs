using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetEwaRequests;

public record GetEwaRequestsQueries : IRequest<Result<List<EwaRequest>>> { }

public class GetEwaRequestsQueriesHandler : IRequestHandler<GetEwaRequestsQueries, Result<List<EwaRequest>>>
{
    private readonly IApplicationDbContext _context;
    public GetEwaRequestsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<EwaRequest>>> Handle(GetEwaRequestsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.EwaRequests.ToListAsync(cancellationToken);
        return Result<List<EwaRequest>>.Success(list);
    }
}