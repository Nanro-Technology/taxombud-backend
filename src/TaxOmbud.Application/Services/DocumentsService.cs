using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Documents.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class DocumentsService : IDocumentsService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    public DocumentsService(
        IApplicationDbContext context,
        IFileStorageService storage
    )
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Response<AddedVersionResponse>> AddDocumentVersionAsync(AddDocumentVersionCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AddedVersionResponse>();
        try
        {
            var doc = await _context.Documents
                .Include(d => d.Versions)
                .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            var nextVersion = doc.Versions.Count + 1;
            var version = new DocumentVersion
            {
                Id = Guid.NewGuid(),
                DocumentId = request.DocumentId,
                VersionNumber = nextVersion,
                FilePath = request.FilePath
            };

            doc.Versions.Add(version);
            doc.FilePath = request.FilePath;

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new AddedVersionResponse(version.Id, version.VersionNumber, version.FilePath);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> ClassifyDocumentAsync(ClassifyDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

            if (document == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            document.Classification = request.Classification;
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<CreatedDocumentResponse>> CreateDocumentAsync(CreateDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreatedDocumentResponse>();
        try
        {
            if (!Enum.TryParse<DocumentEntityType>(request.EntityType, true, out var entityType))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"Invalid entity type '{request.EntityType}'.";
                return response;
            }

            var doc = new Document
            {
                Id = Guid.NewGuid(),
                FileName = request.FileName,
                FilePath = request.FilePath,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                EntityType = entityType,
                EntityId = request.EntityId
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new CreatedDocumentResponse(doc.Id, doc.FileName, doc.FilePath, doc.ContentType, doc.FileSize);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> DeleteDocumentAsync(DeleteDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            await _storage.DeleteAsync(doc.FilePath, cancellationToken);
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<DocumentDetailDto>> GetDocumentByIdAsync(GetDocumentByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<DocumentDetailDto>();
        try
        {
            var doc = await _context.Documents
                .Include(d => d.Versions)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

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

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = dto;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<PagedResult<DocumentListDto>>> GetDocumentsAsync(GetDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<DocumentListDto>>();
        try
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

            var pagedResult = new PagedResult<DocumentListDto>(items.AsReadOnly(), total, request.Page, request.PageSize);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = pagedResult;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<List<DocumentVersionDto>>> GetDocumentVersionsAsync(GetDocumentVersionsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<DocumentVersionDto>>();
        try
        {
            var documentExists = await _context.Documents.AnyAsync(d => d.Id == request.DocumentId, cancellationToken);
            if (!documentExists)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            var versions = await _context.DocumentVersions
                .Where(v => v.DocumentId == request.DocumentId)
                .AsNoTracking()
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DocumentVersionDto(
                    v.Id,
                    v.VersionNumber,
                    v.FilePath,
                    v.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = versions;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<DocumentDownloadUrlDto>> GetDownloadUrlAsync(GetDownloadUrlQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<DocumentDownloadUrlDto>();
        try
        {
            var doc = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            var url = await _storage.GetDownloadUrlAsync(doc.FilePath, cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new DocumentDownloadUrlDto(url);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}
