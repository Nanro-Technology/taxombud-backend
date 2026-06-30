using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Services;

public class ComplaintsService : IComplaintsService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUser _currentUser;

    public ComplaintsService(
        IApplicationDbContext context,
        IFileStorageService storage,
        ICurrentUser currentUser)
    {
        _context = context;
        _storage = storage;
        _currentUser = currentUser;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<ComplaintSummaryDto>>> GetComplaintsAsync(GetComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ComplaintSummaryDto>>();
        try
        {
            var query = _context.Complaints
                .Include(c => c.Taxpayer)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.ReferenceNumber.Contains(request.Search) ||
                                         c.Subject.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            if (!string.IsNullOrWhiteSpace(request.TaxType))
                query = query.Where(c => c.TaxType == request.TaxType);

            if (request.TaxpayerId.HasValue)
                query = query.Where(c => c.TaxpayerId == request.TaxpayerId.Value);

            if (request.AssignedOfficerId.HasValue)
                query = query.Where(c => c.AssignedOfficerId == request.AssignedOfficerId.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ComplaintSummaryDto(
                    c.Id,
                    c.ReferenceNumber,
                    c.Subject,
                    c.TaxType,
                    c.TaxPeriod,
                    c.ComplaintCategory,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.Priority,
                    c.TaxpayerId,
                    c.Taxpayer != null ? $"{c.Taxpayer.FirstName} {c.Taxpayer.LastName}" : null,
                    c.AssignedOfficerId,
                    c.AssignedOfficer != null ? $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}" : null,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints retrieved successfully.";
            response.Data = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving complaints.";
        }
        return response;
    }

    public async Task<Response<PagedResult<ComplaintSummaryDto>>> GetMyComplaintsAsync(GetMyComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ComplaintSummaryDto>>();
        try
        {
            var query = _context.Complaints
                .Include(c => c.Taxpayer)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.ReferenceNumber.Contains(request.Search) ||
                                         c.Subject.Contains(request.Search));

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ComplaintSummaryDto(
                    c.Id,
                    c.ReferenceNumber,
                    c.Subject,
                    c.TaxType,
                    c.TaxPeriod,
                    c.ComplaintCategory,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.Priority,
                    c.TaxpayerId,
                    c.Taxpayer != null ? $"{c.Taxpayer.FirstName} {c.Taxpayer.LastName}" : null,
                    c.AssignedOfficerId,
                    c.AssignedOfficer != null ? $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}" : null,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints retrieved successfully.";
            response.Data = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving complaints.";
        }
        return response;
    }

    public async Task<Response<ComplaintDetailDto>> GetComplaintByIdAsync(GetComplaintByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ComplaintDetailDto>();
        try
        {
            var c = await _context.Complaints
                .Include(x => x.Taxpayer)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (c is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint retrieved successfully.";
            response.Data = new ComplaintDetailDto(
                c.Id,
                c.ReferenceNumber,
                c.Subject,
                c.Description,
                c.TaxType,
                c.TaxPeriod,
                c.ComplaintCategory,
                c.TaxOfficeRef,
                c.TinNumber,
                c.Status.ToString(),
                c.CurrentStage,
                c.Priority,
                c.RequiresApprovalToClose,
                c.ClosedAt,
                c.ClosureReason,
                c.WithdrawalReason,
                new TaxpayerSummary(c.Taxpayer.Id, $"{c.Taxpayer.FirstName} {c.Taxpayer.LastName}", c.Taxpayer.Email.Value, c.Taxpayer.Phone),
                c.AssignedOfficer is null ? null : new OfficerSummary(c.AssignedOfficer.Id, $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}", c.AssignedOfficer.User.Email),
                c.CreatedAt,
                c.UpdatedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the complaint.";
        }
        return response;
    }

    public async Task<Response<ComplaintDetailDto>> GetComplaintByReferenceAsync(GetComplaintByReferenceQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ComplaintDetailDto>();
        try
        {
            var c = await _context.Complaints
                .Include(x => x.Taxpayer)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(x => x.ReferenceNumber == request.ReferenceNumber, cancellationToken);

            if (c is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint retrieved successfully.";
            response.Data = new ComplaintDetailDto(
                c.Id,
                c.ReferenceNumber,
                c.Subject,
                c.Description,
                c.TaxType,
                c.TaxPeriod,
                c.ComplaintCategory,
                c.TaxOfficeRef,
                c.TinNumber,
                c.Status.ToString(),
                c.CurrentStage,
                c.Priority,
                c.RequiresApprovalToClose,
                c.ClosedAt,
                c.ClosureReason,
                c.WithdrawalReason,
                new TaxpayerSummary(c.Taxpayer.Id, $"{c.Taxpayer.FirstName} {c.Taxpayer.LastName}", c.Taxpayer.Email.Value, c.Taxpayer.Phone),
                c.AssignedOfficer is null ? null : new OfficerSummary(c.AssignedOfficer.Id, $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}", c.AssignedOfficer.User.Email),
                c.CreatedAt,
                c.UpdatedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the complaint.";
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<ComplaintNoteDto>>> GetComplaintNotesAsync(GetComplaintNotesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<ComplaintNoteDto>>();
        try
        {
            var notes = await _context.ComplaintNotes
                .Where(n => n.ComplaintId == request.ComplaintId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new ComplaintNoteDto(n.Id, n.Body, n.Visibility, n.AuthorUserId, n.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Notes retrieved successfully.";
            response.Data = notes.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving notes.";
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<ComplaintDocumentDto>>> GetComplaintDocumentsAsync(GetComplaintDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<ComplaintDocumentDto>>();
        try
        {
            var documents = await _context.Documents
                .Where(d => d.EntityType == DocumentEntityType.Complaint && d.EntityId == request.ComplaintId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new ComplaintDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Documents retrieved successfully.";
            response.Data = documents.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving documents.";
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<TimelineEventDto>>> GetComplaintTimelineAsync(GetComplaintTimelineQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<TimelineEventDto>>();
        try
        {
            var history = await _context.ComplaintStatusHistory
                .Where(h => h.ComplaintId == request.ComplaintId)
                .OrderBy(h => h.CreatedAt)
                .Select(h => new TimelineEventDto(
                    "StatusChange",
                    $"Status changed from {h.OldStatus} to {h.NewStatus}",
                    h.OldStatus.ToString(),
                    h.NewStatus.ToString(),
                    h.ChangedByUserId.ToString(),
                    h.TransitionedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Timeline retrieved successfully.";
            response.Data = history.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the timeline.";
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<RelatedComplaintDto>>> GetRelatedComplaintsAsync(GetRelatedComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<RelatedComplaintDto>>();
        try
        {
            var links = await _context.ComplaintLinks
                .Where(l => l.SourceComplaintId == request.ComplaintId || l.TargetComplaintId == request.ComplaintId)
                .Include(l => l.TargetComplaint)
                .Select(l => new RelatedComplaintDto(
                    l.Id,
                    l.TargetComplaintId,
                    l.TargetComplaint.ReferenceNumber,
                    l.TargetComplaint.Subject,
                    l.TargetComplaint.Status.ToString(),
                    l.LinkType
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Related complaints retrieved successfully.";
            response.Data = links.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving related complaints.";
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
                request.TaxpayerId,
                request.TaxType,
                request.TaxPeriod,
                request.ComplaintCategory,
                request.Subject,
                request.Description,
                refNumber,
                request.TaxOfficeRef,
                request.TinNumber
            );

            complaint.Submit();
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint submitted successfully.";
            response.Data = new SubmitComplaintResponse(complaint.Id, refNumber, complaint.Status.ToString());
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while submitting the complaint.";
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
                Id = Guid.NewGuid(),
                ComplaintId = request.ComplaintId,
                Body = request.Body,
                Visibility = request.Visibility,
                AuthorUserId = request.AuthorUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.ComplaintNotes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Note added successfully.";
            response.Data = note.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while adding the note.";
        }
        return response;
    }

    public async Task<Response<object?>> AssignComplaintAsync(AssignComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.ComplaintId }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            complaint.Assign(request.OfficerId, request.AssignedByUserId);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint assigned successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> EscalateComplaintAsync(EscalateComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.ComplaintId }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            complaint.Escalate(request.Reason, request.EscalatedByUserId);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint escalated successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> CloseComplaintAsync(CloseComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.ComplaintId }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            complaint.Close(request.Reason, request.ClosedByUserId);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint closed successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> ReopenComplaintAsync(ReopenComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.ComplaintId }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            complaint.Reopen(request.ReopenedByUserId);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint reopened successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateComplaintAsync(UpdateComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.Id }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            complaint.UpdateDetails(
                request.Subject,
                request.Description,
                request.TaxType,
                request.TaxPeriod,
                request.ComplaintCategory,
                request.TaxOfficeRef,
                request.TinNumber
            );

            complaint.UpdatePriority(request.Priority);
            complaint.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint updated successfully.";
            response.Data = null;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the complaint.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateComplaintStatusAsync(UpdateComplaintStatusCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.ComplaintId }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            var userId = _currentUser.UserId ?? Guid.Empty;

            switch (request.Status)
            {
                case ComplaintStatus.Submitted:
                    complaint.Submit();
                    break;
                case ComplaintStatus.UnderReview:
                    complaint.Reopen(userId);
                    break;
                case ComplaintStatus.Escalated:
                    complaint.Escalate(request.Reason ?? "Status updated to Escalated.", userId);
                    break;
                case ComplaintStatus.Resolved:
                    complaint.Resolve(userId);
                    break;
                case ComplaintStatus.Closed:
                    complaint.Close(request.Reason ?? "Status updated to Closed.", userId);
                    break;
                case ComplaintStatus.Withdrawn:
                    complaint.Withdraw(request.Reason ?? "Status updated to Withdrawn.", userId);
                    break;
                default:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = $"Invalid or unsupported status transition to '{request.Status}'.";
                    return response;
            }

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint status updated.";
            response.Data = null;
        }
        catch (DomainException ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = ex.Message;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the status.";
        }
        return response;
    }

    public async Task<Response<object?>> DeleteComplaintAsync(DeleteComplaintCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var complaint = await _context.Complaints.FindAsync(new object[] { request.Id }, cancellationToken);
            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Complaint not found.";
                return response;
            }

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaint deleted successfully.";
            response.Data = null;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the complaint.";
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

            _context.ComplaintLinks.Add(link);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Complaints linked successfully.";
            response.Data = null;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while linking complaints.";
        }
        return response;
    }

    public async Task<Response<Guid>> UploadComplaintDocumentAsync(UploadComplaintDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            await using var stream = request.File.OpenReadStream();
            var path = await _storage.StoreAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                cancellationToken);

            var docId = Guid.NewGuid();
            var doc = new Document
            {
                Id = docId,
                FileName = request.File.FileName,
                FilePath = path,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                EntityType = DocumentEntityType.Complaint,
                EntityId = request.ComplaintId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Document uploaded successfully.";
            response.Data = docId;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while uploading the document.";
        }
        return response;
    }
}
