using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetQuotes;

public record GetQuotesQueries : IRequest<Result<List<Quote>>> { }

public class GetQuotesQueriesHandler : IRequestHandler<GetQuotesQueries, Result<List<Quote>>>
{
    private readonly IApplicationDbContext _context;
    public GetQuotesQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<Quote>>> Handle(GetQuotesQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.Quotes.ToListAsync(cancellationToken);
        return Result<List<Quote>>.Success(list);
    }
}