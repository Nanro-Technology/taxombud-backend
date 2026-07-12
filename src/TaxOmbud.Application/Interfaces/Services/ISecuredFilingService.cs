using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.SecuredFiling.DTOs;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISecuredFilingService
{
    Task<List<FilingFolderDto>> GetFoldersAsync(string? query, CancellationToken ct = default);
    Task<FilingFolderDto?> GetFolderByIdAsync(Guid id, CancellationToken ct = default);
    Task<FilingFolderDto> CreateFolderAsync(CreateFolderRequest request, CancellationToken ct = default);
    Task<bool> DeleteFoldersAsync(List<Guid> folderIds, CancellationToken ct = default);
    
    Task<List<FilingDocumentDto>> GetDocumentsAsync(Guid? folderId, string? query, CancellationToken ct = default);
    Task<FilingDocumentDto> UploadDocumentAsync(Guid folderId, string fileName, Stream fileStream, string contentType, string? sender, string? senderOrg, CancellationToken ct = default);
    Task<bool> DeleteDocumentsAsync(List<Guid> documentIds, CancellationToken ct = default);
    
    Task<List<FilingInboxRoutingDto>> GetInboxRoutingsAsync(string? query, CancellationToken ct = default);
    Task<bool> AcknowledgeRoutingAsync(Guid id, CancellationToken ct = default);
    Task<bool> RejectRoutingAsync(Guid id, string reason, CancellationToken ct = default);
    
    Task<List<FilingCategoryDto>> GetCategoriesAsync(string? query, CancellationToken ct = default);
    Task<FilingCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    Task<List<AuditLog>> GetSecuredFilingAuditLogsAsync(CancellationToken ct = default);
    Task<bool> ClearSecuredFilingAuditLogsAsync(CancellationToken ct = default);
}
