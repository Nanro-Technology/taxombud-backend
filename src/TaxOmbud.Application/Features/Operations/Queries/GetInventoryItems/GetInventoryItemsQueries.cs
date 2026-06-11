using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetInventoryItems;

public record GetInventoryItemsQueries : IRequest<Result<List<InventoryItem>>> { }

public class GetInventoryItemsQueriesHandler : IRequestHandler<GetInventoryItemsQueries, Result<List<InventoryItem>>>
{
    private readonly IApplicationDbContext _context;
    public GetInventoryItemsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<InventoryItem>>> Handle(GetInventoryItemsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.InventoryItems.ToListAsync(cancellationToken);
        return Result<List<InventoryItem>>.Success(list);
    }
}