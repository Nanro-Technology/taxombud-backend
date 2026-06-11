using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Documents.Queries.GetDownloadUrl;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDownloadUrlQuery(Guid Id) : IRequest<Result<DocumentDownloadUrlDto>>;

public record DocumentDownloadUrlDto(string DownloadUrl);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDownloadUrlQueryHandler : IRequestHandler<GetDownloadUrlQuery, Result<DocumentDownloadUrlDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    public GetDownloadUrlQueryHandler(IApplicationDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result<DocumentDownloadUrlDto>> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var doc = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc == null)
            return Result<DocumentDownloadUrlDto>.NotFound("Document not found.");

        var url = await _storage.GetDownloadUrlAsync(doc.FilePath, cancellationToken);
        return Result<DocumentDownloadUrlDto>.Success(new DocumentDownloadUrlDto(url));
    }
}
