using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Queries.GetCaseDocuments;

public record GetCaseDocumentsQuery(Guid CaseId)
    : IRequest<Result<IReadOnlyList<CaseDocumentDto>>>;

public record CaseDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt
);

public class GetCaseDocumentsQueryHandler
    : IRequestHandler<GetCaseDocumentsQuery, Result<IReadOnlyList<CaseDocumentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CaseDocumentDto>>> Handle(
        GetCaseDocumentsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Cases
            .AnyAsync(c => c.Id == request.CaseId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<CaseDocumentDto>>.NotFound(
                $"Case '{request.CaseId}' was not found.");

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(d => d.EntityType == DocumentEntityType.Case && d.EntityId == request.CaseId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new CaseDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CaseDocumentDto>>.Success(documents.AsReadOnly());
    }
}
