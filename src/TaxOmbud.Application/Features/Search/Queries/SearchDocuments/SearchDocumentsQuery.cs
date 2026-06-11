using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Search.Queries.SearchDocuments;

public record SearchDocumentsQuery(string Term) : IRequest<Result<List<DocumentSearchResultDto>>>;

public record DocumentSearchResultDto(global::System.Guid Id, string FileName, string Classification);

public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, Result<List<DocumentSearchResultDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DocumentSearchResultDto>>> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        var term = $"%{request.Term}%";
        var results = await _context.Documents
            .AsNoTracking()
            .Where(d => EF.Functions.Like(d.FileName, term) || 
                        (d.Classification != null && EF.Functions.Like(d.Classification, term)))
            .Select(d => new DocumentSearchResultDto(d.Id, d.FileName, d.Classification ?? ""))
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result<List<DocumentSearchResultDto>>.Success(results);
    }
}
