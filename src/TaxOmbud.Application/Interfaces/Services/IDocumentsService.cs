using TaxOmbud.Application.Documents.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IDocumentsService
{
    Task<Response<AddedVersionResponse>> AddDocumentVersionAsync(AddDocumentVersionCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ClassifyDocumentAsync(ClassifyDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<CreatedDocumentResponse>> CreateDocumentAsync(CreateDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteDocumentAsync(DeleteDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<DocumentDetailDto>> GetDocumentByIdAsync(GetDocumentByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<DocumentListDto>>> GetDocumentsAsync(GetDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<DocumentVersionDto>>> GetDocumentVersionsAsync(GetDocumentVersionsQuery request, CancellationToken cancellationToken = default);
    Task<Response<DocumentDownloadUrlDto>> GetDownloadUrlAsync(GetDownloadUrlQuery request, CancellationToken cancellationToken = default);
}
