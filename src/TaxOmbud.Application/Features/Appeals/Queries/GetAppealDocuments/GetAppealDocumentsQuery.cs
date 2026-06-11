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

namespace TaxOmbud.Application.Features.Appeals.Queries.GetAppealDocuments;

public record GetAppealDocumentsQuery(Guid AppealId)
    : IRequest<Result<IReadOnlyList<AppealDocumentDto>>>;

public record AppealDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt
);

public class GetAppealDocumentsQueryHandler
    : IRequestHandler<GetAppealDocumentsQuery, Result<IReadOnlyList<AppealDocumentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppealDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<AppealDocumentDto>>> Handle(
        GetAppealDocumentsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Appeals
            .AnyAsync(a => a.Id == request.AppealId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<AppealDocumentDto>>.NotFound(
                $"Appeal '{request.AppealId}' was not found.");

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(d => d.EntityType == DocumentEntityType.Appeal && d.EntityId == request.AppealId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new AppealDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AppealDocumentDto>>.Success(documents.AsReadOnly());
    }
}
