using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Application.Services;

public class ComplaintsService : IComplaintsService
{
    private readonly IGenericRepository<Complaint> _complaintRepo;
    private readonly IGenericRepository<Case> _caseRepo;
    private readonly IGenericRepository<ComplaintNote> _noteRepo;
    private readonly IGenericRepository<ComplaintStatusHistory> _historyRepo;
    private readonly IGenericRepository<ComplaintLink> _linkRepo;
    private readonly IGenericRepository<Document> _documentRepo;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUser _currentUser;

    public ComplaintsService(
        IGenericRepository<Complaint> complaintRepo,
        IGenericRepository<Case> caseRepo,
        IGenericRepository<ComplaintNote> noteRepo,
        IGenericRepository<ComplaintStatusHistory> historyRepo,
        IGenericRepository<ComplaintLink> linkRepo,
        IGenericRepository<Document> documentRepo,
        IFileStorageService storage,
        ICurrentUser currentUser)
    {
        _complaintRepo = complaintRepo;
        _caseRepo = caseRepo;
        _noteRepo = noteRepo;
        _historyRepo = historyRepo;
        _linkRepo = linkRepo;
        _documentRepo = documentRepo;
        _storage = storage;
        _currentUser = currentUser;
    }

    // ─── Queries ────────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<ComplaintSummaryDto>>> GetComplaintsAsync(GetComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ComplaintSummaryDto>>();
        try
        {
            var query = _complaintRepo.Query()
                .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.ReferenceNumber.Contains(request.Search) || c.Subject.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            if (!string.IsNullOrWhiteSpace(request.TaxType))
                query = query.Where(c => c.TaxType == request.TaxType);

            if (request.TaxpayerId.HasValue)
                query = query.Where(c => c.TaxpayerId == request.TaxpayerId.Value);

            if (request.AssignedOfficerId.HasValue)
                query = query.Where(c => c.AssignedOfficerId == request.AssignedOfficerId.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                .Select(c => new ComplaintSummaryDto(
                    c.Id, c.ReferenceNumber, c.Subject, c.TaxType, c.TaxPeriod, c.ComplaintCategory,
                    c.Status.ToString(), c.CurrentStage, c.Priority, c.TaxpayerId,
                    c.Taxpayer != null && c.Taxpayer.User != null ? $"{c.Taxpayer.User.FirstName} {c.Taxpayer.User.LastName}" : null,
                    c.AssignedOfficerId,
                    c.AssignedOfficer != null ? $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}" : null,
                    c.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintsRetrieved;
            response.Data = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintRetrieveError;
        }
        return response;
    }

    public async Task<Response<PagedResult<ComplaintSummaryDto>>> GetMyComplaintsAsync(GetMyComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ComplaintSummaryDto>>();
        try
        {
            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
            {
                response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                response.Message = "User is not authenticated.";
                return response;
            }

            var query = _complaintRepo.Query()
                .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .Where(c => c.Taxpayer.UserId == currentUserId.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.ReferenceNumber.Contains(request.Search) || c.Subject.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                .Select(c => new ComplaintSummaryDto(
                    c.Id, c.ReferenceNumber, c.Subject, c.TaxType, c.TaxPeriod, c.ComplaintCategory,
                    c.Status.ToString(), c.CurrentStage, c.Priority, c.TaxpayerId,
                    c.Taxpayer != null && c.Taxpayer.User != null ? $"{c.Taxpayer.User.FirstName} {c.Taxpayer.User.LastName}" : null,
                    c.AssignedOfficerId,
                    c.AssignedOfficer != null ? $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}" : null,
                    c.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintsRetrieved;
            response.Data = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintRetrieveError;
        }
        return response;
    }

    public async Task<Response<ComplaintDetailDto>> GetComplaintByIdAsync(GetComplaintByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ComplaintDetailDto>();
        try
        {
            var c = await _complaintRepo.Query()
                .Include(x => x.Taxpayer).ThenInclude(tp => tp.User)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (c is null)
            {
                // Fallback: check if request.Id was passed as Case.Id or Case.ComplaintId
                var caseItem = await _caseRepo.Query().FirstOrDefaultAsync(cs => cs.Id == request.Id || cs.ComplaintId == request.Id, cancellationToken);
                if (caseItem != null)
                {
                    c = await _complaintRepo.Query()
                        .Include(x => x.Taxpayer).ThenInclude(tp => tp.User)
                        .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                        .FirstOrDefaultAsync(x => x.Id == caseItem.ComplaintId, cancellationToken);
                }
            }

            if (c is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintRetrieved;
            response.Data = MapToDetail(c);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintGetError;
        }
        return response;
    }

    public async Task<Response<ComplaintDetailDto>> GetComplaintByReferenceAsync(GetComplaintByReferenceQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ComplaintDetailDto>();
        try
        {
            var c = await _complaintRepo.Query()
                .Include(x => x.Taxpayer).ThenInclude(tp => tp.User)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(x => x.ReferenceNumber == request.ReferenceNumber, cancellationToken);

            if (c is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintRetrieved;
            response.Data = MapToDetail(c);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintGetError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<ComplaintNoteDto>>> GetComplaintNotesAsync(GetComplaintNotesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<ComplaintNoteDto>>();
        try
        {
            var notes = await _noteRepo.Query()
                .Where(n => n.ComplaintId == request.ComplaintId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new ComplaintNoteDto(n.Id, n.Body, n.Visibility, n.AuthorUserId, n.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.NotesRetrieved;
            response.Data = notes.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintNotesError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<ComplaintDocumentDto>>> GetComplaintDocumentsAsync(GetComplaintDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<ComplaintDocumentDto>>();
        try
        {
            var documents = await _documentRepo.Query()
                .Where(d => d.EntityType == DocumentEntityType.Complaint && d.EntityId == request.ComplaintId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new ComplaintDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentsRetrieved;
            response.Data = documents.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintDocsError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<TimelineEventDto>>> GetComplaintTimelineAsync(GetComplaintTimelineQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<TimelineEventDto>>();
        try
        {
            var history = await _historyRepo.Query()
                .Where(h => h.ComplaintId == request.ComplaintId)
                .OrderBy(h => h.CreatedAt)
                .Select(h => new TimelineEventDto(
                    "StatusChange", $"Status changed from {h.OldStatus} to {h.NewStatus}",
                    h.OldStatus.ToString(), h.NewStatus.ToString(), h.ChangedByUserId.ToString(), h.TransitionedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.TimelineRetrieved;
            response.Data = history.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintTimelineError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<RelatedComplaintDto>>> GetRelatedComplaintsAsync(GetRelatedComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<RelatedComplaintDto>>();
        try
        {
            var links = await _linkRepo.Query()
                .Where(l => l.SourceComplaintId == request.ComplaintId || l.TargetComplaintId == request.ComplaintId)
                .Include(l => l.TargetComplaint)
                .Select(l => new RelatedComplaintDto(
                    l.Id, l.TargetComplaintId, l.TargetComplaint.ReferenceNumber,
                    l.TargetComplaint.Subject, l.TargetComplaint.Status.ToString(), l.LinkType))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.RelatedComplaintsRetrieved;
            response.Data = links.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintRelatedError;
        }
        return response;
    }

    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<SubmitComplaintResponse>> SubmitComplaintAsync(SubmitComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SubmitComplaintResponse>();
        try
        {
            var refNumber = $"TOC-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var complaint = Complaint.Create(
                request.TaxpayerId, request.TaxType, request.TaxPeriod, request.ComplaintCategory,
                request.Subject, request.Description, refNumber, request.TaxOfficeRef, request.TinNumber);

            complaint.Submit();
            await _complaintRepo.AddAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintSubmitted;
            response.Data = new SubmitComplaintResponse(complaint.Id, refNumber, complaint.Status.ToString());
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintSubmitError;
        }
        return response;
    }

    public async Task<Response<Guid>> AddComplaintNoteAsync(AddComplaintNoteCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var note = new ComplaintNote
            {
                Id = Guid.NewGuid(), ComplaintId = request.ComplaintId, Body = request.Body,
                Visibility = request.Visibility, AuthorUserId = request.AuthorUserId, CreatedAt = DateTime.UtcNow
            };
            await _noteRepo.AddAsync(note);
            await _noteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.NoteAdded;
            response.Data = note.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintNoteAddError;
        }
        return response;
    }

    public async Task<Response<object?>> AssignComplaintAsync(AssignComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.ComplaintId);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            complaint.Assign(request.OfficerId, request.AssignedByUserId);
            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintAssigned;
        }
        catch (DomainException ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> EscalateComplaintAsync(EscalateComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.ComplaintId);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            complaint.Escalate(request.Reason, request.EscalatedByUserId);
            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintEscalated;
        }
        catch (DomainException ex) { response.StatusCode = StatusCodes.Status400BadRequest; response.Message = ex.Message; }
        catch (Exception) { response.StatusCode = StatusCodes.Status500InternalServerError; response.Message = Constants.Messages.ServerError; }
        return response;
    }

    public async Task<Response<object?>> CloseComplaintAsync(CloseComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.ComplaintId);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            complaint.Close(request.Reason, request.ClosedByUserId);
            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintClosed;
        }
        catch (DomainException ex) { response.StatusCode = StatusCodes.Status400BadRequest; response.Message = ex.Message; }
        catch (Exception) { response.StatusCode = StatusCodes.Status500InternalServerError; response.Message = Constants.Messages.ServerError; }
        return response;
    }

    public async Task<Response<object?>> ReopenComplaintAsync(ReopenComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.ComplaintId);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            complaint.Reopen(request.ReopenedByUserId);
            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintReopened;
        }
        catch (DomainException ex) { response.StatusCode = StatusCodes.Status400BadRequest; response.Message = ex.Message; }
        catch (Exception) { response.StatusCode = StatusCodes.Status500InternalServerError; response.Message = Constants.Messages.ServerError; }
        return response;
    }

    public async Task<Response<object?>> UpdateComplaintAsync(UpdateComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.Id);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            complaint.UpdateDetails(request.Subject, request.Description, request.TaxType, request.TaxPeriod,
                request.ComplaintCategory, request.TaxOfficeRef, request.TinNumber);
            complaint.UpdatePriority(request.Priority);
            complaint.LastModifiedAt = DateTime.UtcNow;

            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintUpdated;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintUpdateError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateComplaintStatusAsync(UpdateComplaintStatusCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.ComplaintId);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            var userId = _currentUser.UserId ?? Guid.Empty;
            switch (request.Status)
            {
                case ComplaintStatus.Submitted: complaint.Submit(); break;
                case ComplaintStatus.Registered: complaint.Reopen(userId); break;
                case ComplaintStatus.UnderAssessment: complaint.Reopen(userId); break;
                case ComplaintStatus.Assigned: complaint.Assign(userId, userId); break;
                case ComplaintStatus.UnderInvestigation: complaint.Escalate(request.Reason ?? "Status updated to UnderInvestigation.", userId); break;
                case ComplaintStatus.DecisionIssued: complaint.Resolve(userId); break;
                case ComplaintStatus.Closed: complaint.Close(request.Reason ?? "Status updated to Closed.", userId); break;
                case ComplaintStatus.Withdrawn: complaint.Withdraw(request.Reason ?? "Status updated to Withdrawn.", userId); break;
                default:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = $"Invalid or unsupported status transition to '{request.Status}'.";
                    return response;
            }


            await _complaintRepo.UpdateAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintStatusUpdated;
        }
        catch (DomainException ex) { response.StatusCode = StatusCodes.Status400BadRequest; response.Message = ex.Message; }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintStatusUpdateError;
        }
        return response;
    }

    public async Task<Response<object?>> DeleteComplaintAsync(DeleteComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _complaintRepo.GetByIdAsync(request.Id);
            if (complaint is null) { response.StatusCode = StatusCodes.Status404NotFound; response.Message = Constants.Messages.ComplaintNotFound; return response; }

            await _complaintRepo.RemoveAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintDeleted;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintDeleteError;
        }
        return response;
    }

    public async Task<Response<object?>> LinkComplaintsAsync(LinkComplaintsCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var link = new ComplaintLink
            {
                Id = Guid.NewGuid(),
                SourceComplaintId = request.SourceComplaintId,
                TargetComplaintId = request.TargetComplaintId,
                LinkType = request.LinkType ?? "related",
                LinkedByUserId = _currentUser.UserId ?? Guid.Empty
            };
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintsLinked;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintLinkError;
        }
        return response;
    }

    public async Task<Response<Guid>> UploadComplaintDocumentAsync(UploadComplaintDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            await using var stream = request.File.OpenReadStream();
            var path = await _storage.StoreAsync(stream, request.File.FileName, request.File.ContentType, cancellationToken);

            var docId = Guid.NewGuid();
            var doc = new Document
            {
                Id = docId, FileName = request.File.FileName, FilePath = path,
                ContentType = request.File.ContentType, FileSize = request.File.Length,
                EntityType = DocumentEntityType.Complaint, EntityId = request.ComplaintId, CreatedAt = DateTime.UtcNow
            };
            await _documentRepo.AddAsync(doc);
            await _documentRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentUploaded;
            response.Data = docId;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintDocUploadError;
        }
        return response;
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private static ComplaintDetailDto MapToDetail(Complaint c) => new(
        c.Id, c.ReferenceNumber, c.Subject, c.Description, c.TaxType, c.TaxPeriod, c.ComplaintCategory,
        c.TaxOfficeRef, c.TinNumber, c.Status.ToString(), c.CurrentStage, c.Priority,
        c.RequiresApprovalToClose, c.ClosedAt, c.ClosureReason, c.WithdrawalReason,
        new TaxpayerSummary(
            c.Taxpayer.Id,
            c.Taxpayer.User != null ? $"{c.Taxpayer.User.FirstName} {c.Taxpayer.User.LastName}" : "Unknown",
            c.Taxpayer.User?.Email,
            c.Taxpayer.User?.Phone
        ),
        c.AssignedOfficer is null ? null : new OfficerSummary(c.AssignedOfficer.Id, $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}", c.AssignedOfficer.User.Email),
        c.CreatedAt, c.LastModifiedAt
    );
}
