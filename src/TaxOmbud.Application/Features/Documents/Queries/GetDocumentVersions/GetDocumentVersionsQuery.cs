using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Features.Documents.Queries.GetDocumentVersions;

public record GetDocumentVersionsQuery(Guid DocumentId) : IRequest<Result<List<DocumentVersionDto>>>;

public record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    string FilePath,
    long FileSize,
    DateTimeOffset CreatedAt
);

public class GetDocumentVersionsQueryHandler : IRequestHandler<GetDocumentVersionsQuery, Result<List<DocumentVersionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentVersionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DocumentVersionDto>>> Handle(GetDocumentVersionsQuery request, CancellationToken cancellationToken)
    {
        var documentExists = await _context.Documents.AnyAsync(d => d.Id == request.DocumentId, cancellationToken);
        if (!documentExists)
            throw new NotFoundException(nameof(Domain.Entities.Documents.Document), request.DocumentId);

        var versions = await _context.DocumentVersions
            .Where(v => v.DocumentId == request.DocumentId)
            .AsNoTracking()
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDto(
                v.Id,
                v.VersionNumber,
                v.FilePath,
                v.FileSize,
                v.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<DocumentVersionDto>>.Success(versions);
    }
}
