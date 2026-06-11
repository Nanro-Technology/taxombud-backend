using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Search.Queries.SearchTaxpayers;

public record SearchTaxpayersQuery(string Term) : IRequest<Result<List<TaxpayerSearchResultDto>>>;

public record TaxpayerSearchResultDto(global::System.Guid Id, string Tin, string EntityName);

public class SearchTaxpayersQueryHandler : IRequestHandler<SearchTaxpayersQuery, Result<List<TaxpayerSearchResultDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchTaxpayersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TaxpayerSearchResultDto>>> Handle(SearchTaxpayersQuery request, CancellationToken cancellationToken)
    {
        var term = $"%{request.Term}%";
        var results = await _context.Taxpayers
            .AsNoTracking()
            .Where(t => (t.TaxId != null && EF.Functions.Like(EF.Property<string>(t, "TaxId"), term)) || 
                        EF.Functions.Like(t.FirstName + " " + t.LastName, term))
            .Select(t => new TaxpayerSearchResultDto(t.Id, t.TaxId != null ? t.TaxId.Value : "", t.FirstName + " " + t.LastName))
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result<List<TaxpayerSearchResultDto>>.Success(results);
    }
}
