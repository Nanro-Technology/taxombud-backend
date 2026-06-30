using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Documents.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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
