using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetInvoices;

public record GetInvoicesQueries : IRequest<Result<List<Invoice>>> { }

public class GetInvoicesQueriesHandler : IRequestHandler<GetInvoicesQueries, Result<List<Invoice>>>
{
    private readonly IApplicationDbContext _context;
    public GetInvoicesQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<Invoice>>> Handle(GetInvoicesQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.Invoices.ToListAsync(cancellationToken);
        return Result<List<Invoice>>.Success(list);
    }
}