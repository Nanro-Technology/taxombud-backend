using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Search.Queries.GlobalSearch;

public record GlobalSearchQuery(string Query, int Top = 10) : IRequest<Result<GlobalSearchResultDto>>;

public record GlobalSearchResultDto(
    IReadOnlyList<SearchResultItem> Complaints,
    IReadOnlyList<SearchResultItem> Cases,
    IReadOnlyList<SearchResultItem> Taxpayers
);

public record SearchResultItem(Guid Id, string Title, string Description, string EntityType, string Url);

public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, Result<GlobalSearchResultDto>>
{
    private readonly IApplicationDbContext _context;

    public GlobalSearchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GlobalSearchResultDto>> Handle(
        GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result<GlobalSearchResultDto>.Success(new GlobalSearchResultDto(
                new List<SearchResultItem>(), new List<SearchResultItem>(), new List<SearchResultItem>()));

        var searchTerm = $"%{request.Query}%";

        var complaints = await _context.Complaints
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.ReferenceNumber, searchTerm) || EF.Functions.Like(c.Subject, searchTerm))
            .Take(request.Top)
            .Select(c => new SearchResultItem(
                c.Id, 
                c.ReferenceNumber, 
                c.Subject, 
                "Complaint", 
                $"/api/v1/complaints/{c.Id}"))
            .ToListAsync(cancellationToken);

        var cases = await _context.Cases
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.CaseNumber.Value, searchTerm) || EF.Functions.Like(c.Subject, searchTerm))
            .Take(request.Top)
            .Select(c => new SearchResultItem(
                c.Id, 
                c.CaseNumber.Value, 
                c.Subject, 
                "Case", 
                $"/api/v1/cases/{c.Id}"))
            .ToListAsync(cancellationToken);

        var taxpayers = await _context.Taxpayers
            .AsNoTracking()
            .Where(t => EF.Functions.Like(t.FirstName, searchTerm) 
                     || EF.Functions.Like(t.LastName, searchTerm) 
                     || (t.Nin != null && EF.Functions.Like(t.Nin, searchTerm)))
            .Take(request.Top)
            .Select(t => new SearchResultItem(
                t.Id, 
                $"{t.FirstName} {t.LastName}", 
                t.Email.Value, 
                "Taxpayer", 
                $"/api/v1/taxpayers/{t.Id}"))
            .ToListAsync(cancellationToken);

        var result = new GlobalSearchResultDto(complaints, cases, taxpayers);

        return Result<GlobalSearchResultDto>.Success(result);
    }
}
