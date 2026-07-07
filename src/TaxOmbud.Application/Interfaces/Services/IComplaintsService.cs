using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IComplaintsService
{
    Task<Response<Guid>> AddComplaintNoteAsync(AddComplaintNoteCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> AssignComplaintAsync(AssignComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> CloseComplaintAsync(CloseComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteComplaintAsync(DeleteComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> EscalateComplaintAsync(EscalateComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> LinkComplaintsAsync(LinkComplaintsCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ReopenComplaintAsync(ReopenComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<SubmitComplaintResponse>> SubmitComplaintAsync(SubmitComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateComplaintAsync(UpdateComplaintCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateComplaintStatusAsync(UpdateComplaintStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UploadComplaintDocumentAsync(UploadComplaintDocumentCommand request, CancellationToken cancellationToken = default);
    Task<Response<ComplaintDetailDto>> GetComplaintByIdAsync(GetComplaintByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<ComplaintDetailDto>> GetComplaintByReferenceAsync(GetComplaintByReferenceQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<ComplaintDocumentDto>>> GetComplaintDocumentsAsync(GetComplaintDocumentsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<ComplaintNoteDto>>> GetComplaintNotesAsync(GetComplaintNotesQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<ComplaintSummaryDto>>> GetComplaintsAsync(GetComplaintsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<TimelineEventDto>>> GetComplaintTimelineAsync(GetComplaintTimelineQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<ComplaintSummaryDto>>> GetMyComplaintsAsync(GetMyComplaintsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<RelatedComplaintDto>>> GetRelatedComplaintsAsync(GetRelatedComplaintsQuery request, CancellationToken cancellationToken = default);
}
