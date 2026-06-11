using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetRemittances;

public record GetRemittancesQueries : IRequest<Result<List<Remittance>>> { }

public class GetRemittancesQueriesHandler : IRequestHandler<GetRemittancesQueries, Result<List<Remittance>>>
{
    private readonly IApplicationDbContext _context;
    public GetRemittancesQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<Remittance>>> Handle(GetRemittancesQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.Remittances.ToListAsync(cancellationToken);
        return Result<List<Remittance>>.Success(list);
    }
}