using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Search.Queries.SearchCases;

public record SearchCasesQuery(string Term) : IRequest<Result<List<CaseSearchResultDto>>>;

public record CaseSearchResultDto(global::System.Guid Id, string ReferenceNumber, string Status);

public class SearchCasesQueryHandler : IRequestHandler<SearchCasesQuery, Result<List<CaseSearchResultDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchCasesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CaseSearchResultDto>>> Handle(SearchCasesQuery request, CancellationToken cancellationToken)
    {
        var term = $"%{request.Term}%";
        var results = await _context.Cases
            .AsNoTracking()
            .Where(c => c.CaseNumber != null && EF.Functions.Like(EF.Property<string>(c, "CaseNumber"), term))
            .Select(c => new CaseSearchResultDto(c.Id, c.CaseNumber != null ? c.CaseNumber.Value : "", c.Status.ToString()))
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result<List<CaseSearchResultDto>>.Success(results);
    }
}
