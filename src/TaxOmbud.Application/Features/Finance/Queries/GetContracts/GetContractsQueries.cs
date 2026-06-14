using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetContracts;

public record GetContractsQueries : IRequest<Result<List<Contract>>> { }

public class GetContractsQueriesHandler : IRequestHandler<GetContractsQueries, Result<List<Contract>>>
{
    private readonly IApplicationDbContext _context;
    public GetContractsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<Contract>>> Handle(GetContractsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.Contracts.ToListAsync(cancellationToken);
        return Result<List<Contract>>.Success(list);
    }
}