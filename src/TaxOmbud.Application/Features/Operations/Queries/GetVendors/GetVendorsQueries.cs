using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Queries.GetVendors;

public record GetVendorsQueries : IRequest<Result<List<VendorContact>>> { }

public class GetVendorsQueriesHandler : IRequestHandler<GetVendorsQueries, Result<List<VendorContact>>>
{
    private readonly IApplicationDbContext _context;
    public GetVendorsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<VendorContact>>> Handle(GetVendorsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.VendorContacts.ToListAsync(cancellationToken);
        return Result<List<VendorContact>>.Success(list);
    }
}