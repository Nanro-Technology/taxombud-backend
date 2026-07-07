using TaxOmbud.Application.Appeals.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IAppealsService
{
    Task<Response<FileAppealResponse>> FileAppealAsync(FileAppealCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ReviewAppealAsync(ReviewAppealCommand request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UploadAppealDocumentAsync(UploadAppealDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<AppealDetailDto>> GetAppealByIdAsync(GetAppealByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<AppealDocumentDto>>> GetAppealDocumentsAsync(GetAppealDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AppealListDto>>> GetAppealsAsync(GetAppealsQuery request, CancellationToken cancellationToken = default);
}
