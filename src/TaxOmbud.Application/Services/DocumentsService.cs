using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Documents.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class DocumentsService : IDocumentsService
{
    private readonly IGenericRepository<Document> _docRepo;
    private readonly IGenericRepository<DocumentVersion> _versionRepo;
    private readonly IFileStorageService _storage;

    public DocumentsService(
        IGenericRepository<Document> docRepo,
        IGenericRepository<DocumentVersion> versionRepo,
        IFileStorageService storage)
    {
        _docRepo = docRepo;
        _versionRepo = versionRepo;
        _storage = storage;
    }

    public async Task<Response<AddedVersionResponse>> AddDocumentVersionAsync(AddDocumentVersionCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AddedVersionResponse>();
        try
        {
            var doc = await _docRepo.Query()
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

            await _docRepo.UpdateAsync(doc);
            await _docRepo.SaveAsync();

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
            var document = await _docRepo.FindAsync(d => d.Id == request.DocumentId);
            if (document == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            document.Classification = request.Classification;
            await _docRepo.UpdateAsync(document);
            await _docRepo.SaveAsync();

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

            await _docRepo.AddAsync(doc);
            await _docRepo.SaveAsync();

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
            var doc = await _docRepo.FindAsync(d => d.Id == request.Id);
            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            await _storage.DeleteAsync(doc.FilePath, cancellationToken);
            await _docRepo.RemoveAsync(doc);
            await _docRepo.SaveAsync();

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
            var doc = await _docRepo.Query()
                .Include(d => d.Versions)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (doc == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new DocumentDetailDto(
                doc.Id, doc.FileName, doc.ContentType, doc.FileSize,
                doc.EntityType.ToString(), doc.EntityId, doc.FilePath, doc.CreatedAt,
                doc.Versions.Select(v => new DocumentVersionDto(v.Id, v.VersionNumber, v.FilePath, v.CreatedAt))
            );
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
            var query = _docRepo.Query().AsNoTracking();

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
                    d.Id, d.FileName, d.ContentType, d.FileSize,
                    d.EntityType.ToString(), d.EntityId, d.FilePath, d.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new PagedResult<DocumentListDto>(items.AsReadOnly(), total, request.Page, request.PageSize);
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
            if (!await _docRepo.ExistsAsync(d => d.Id == request.DocumentId))
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Document not found.";
                return response;
            }

            var versions = await _versionRepo.Query()
                .Where(v => v.DocumentId == request.DocumentId)
                .AsNoTracking()
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DocumentVersionDto(v.Id, v.VersionNumber, v.FilePath, v.CreatedAt))
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
            var doc = await _docRepo.FindAsync(d => d.Id == request.Id);
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
