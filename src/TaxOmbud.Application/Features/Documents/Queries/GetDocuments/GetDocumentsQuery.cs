using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Documents.Queries.GetDocuments;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDocumentsQuery(
    Guid? EntityId,
    string? EntityType,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<DocumentListDto>>>;

public record DocumentListDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId,
    string FilePath,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, Result<PagedResult<DocumentListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<DocumentListDto>>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Documents.AsNoTracking().AsQueryable();

        if (request.EntityId.HasValue)
            query = query.Where(d => d.EntityId == request.EntityId.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityType) && Enum.TryParse<DocumentEntityType>(request.EntityType, true, out var et))
            query = query.Where(d => d.EntityType == et);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DocumentListDto(
                d.Id,
                d.FileName,
                d.ContentType,
                d.FileSize,
                d.EntityType.ToString(),
                d.EntityId,
                d.FilePath,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<DocumentListDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<DocumentListDto>>.Success(pagedResult);
    }
}
