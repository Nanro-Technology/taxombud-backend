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

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaintDocuments;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintDocumentsQuery(Guid ComplaintId)
    : IRequest<Result<IReadOnlyList<ComplaintDocumentDto>>>;

public record ComplaintDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintDocumentsQueryHandler
    : IRequestHandler<GetComplaintDocumentsQuery, Result<IReadOnlyList<ComplaintDocumentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ComplaintDocumentDto>>> Handle(
        GetComplaintDocumentsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<ComplaintDocumentDto>>.NotFound(
                $"Complaint '{request.ComplaintId}' was not found.");

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(d => d.EntityType == DocumentEntityType.Complaint && d.EntityId == request.ComplaintId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new ComplaintDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ComplaintDocumentDto>>.Success(documents.AsReadOnly());
    }
}
