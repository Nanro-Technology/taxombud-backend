using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Search.Queries.SearchComplaints;

public record SearchComplaintsQuery(string Term) : IRequest<Result<List<ComplaintSearchResultDto>>>;

public record ComplaintSearchResultDto(global::System.Guid Id, string ReferenceNumber, string TaxType, string Subject);

public class SearchComplaintsQueryHandler : IRequestHandler<SearchComplaintsQuery, Result<List<ComplaintSearchResultDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchComplaintsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ComplaintSearchResultDto>>> Handle(SearchComplaintsQuery request, CancellationToken cancellationToken)
    {
        var term = $"%{request.Term}%";
        var results = await _context.Complaints
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.ReferenceNumber, term) || 
                        EF.Functions.Like(c.TaxType, term))
            .Select(c => new ComplaintSearchResultDto(c.Id, c.ReferenceNumber, c.TaxType, c.Subject))
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result<List<ComplaintSearchResultDto>>.Success(results);
    }
}
