using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Documents.Queries.GetDocumentById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDocumentByIdQuery(Guid Id) : IRequest<Result<DocumentDetailDto>>;

public record DocumentDetailDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId,
    string FilePath,
    DateTimeOffset CreatedAt,
    IEnumerable<DocumentVersionDto> Versions
);

public record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    string FilePath,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DocumentDetailDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await _context.Documents
            .Include(d => d.Versions)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (doc == null)
            return Result<DocumentDetailDto>.NotFound("Document not found.");

        var dto = new DocumentDetailDto(
            doc.Id,
            doc.FileName,
            doc.ContentType,
            doc.FileSize,
            doc.EntityType.ToString(),
            doc.EntityId,
            doc.FilePath,
            doc.CreatedAt,
            doc.Versions.Select(v => new DocumentVersionDto(v.Id, v.VersionNumber, v.FilePath, v.CreatedAt))
        );

        return Result<DocumentDetailDto>.Success(dto);
    }
}
