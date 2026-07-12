using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ICasesService
{
    Task<Response<Guid>> AddCaseFindingAsync(AddCaseFindingCommand request, CancellationToken cancellationToken = default);
    Task<Response<AddCaseNoteResponse>> AddCaseNoteAsync(AddCaseNoteCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ApproveClosureAsync(ApproveClosureCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> AssignCaseAsync(AssignCaseCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> CompleteMilestoneAsync(CompleteMilestoneCommand request, CancellationToken cancellationToken = default);
    Task<Response<PostRecommendationResponse>> PostRecommendationAsync(PostRecommendationCommand request, CancellationToken cancellationToken = default);
    Task<Response<SubmitPublicCaseResponse>> SubmitPublicCaseAsync(SubmitPublicCaseCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> TransitionCaseAsync(TransitionCaseCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateCaseFindingAsync(UpdateCaseFindingCommand request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UploadCaseDocumentAsync(UploadCaseDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<CaseDetailDto>> GetCaseByIdAsync(GetCaseByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<CaseCommunicationDto>>> GetCaseCommunicationsAsync(GetCaseCommunicationsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<CaseDocumentDto>>> GetCaseDocumentsAsync(GetCaseDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<CaseFindingDto>>> GetCaseFindingsAsync(GetCaseFindingsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<CaseMilestoneDto>>> GetCaseMilestonesAsync(GetCaseMilestonesQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<CaseListDto>>> GetCasesAsync(GetCasesQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<CaseListDto>>> GetMyCasesAsync(GetMyCasesQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<CaseListDto>>> GetOverdueCasesAsync(GetOverdueCasesQuery request, CancellationToken cancellationToken = default);
    Task<Response<QueueResultDto>> GetQueueAsync(GetQueueQuery request, CancellationToken cancellationToken = default);
    Task<Response<TrackComplaintResponse>> TrackComplaintAsync(TrackComplaintQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<CaseHistoryListDto>>> GetCaseHistoryAsync(GetCaseHistoryQuery request, CancellationToken cancellationToken = default);
}
