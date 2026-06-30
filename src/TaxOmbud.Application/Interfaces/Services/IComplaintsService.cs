using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Complaints.DTOs;
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
