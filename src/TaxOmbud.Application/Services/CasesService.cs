using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Cases.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Application.Services;

public class CasesService : ICasesService
{
    private readonly IGenericRepository<Case> _caseRepo;
    private readonly IGenericRepository<Complaint> _complaintRepo;
    private readonly IGenericRepository<CaseFinding> _findingRepo;
    private readonly IGenericRepository<CaseMilestone> _milestoneRepo;
    private readonly IGenericRepository<CaseCommunicationLog> _communicationRepo;
    private readonly IGenericRepository<Document> _docRepo;
    private readonly IGenericRepository<CaseNote> _noteRepo;

    public CasesService(
        IGenericRepository<Case> caseRepo,
        IGenericRepository<Complaint> complaintRepo,
        IGenericRepository<CaseFinding> findingRepo,
        IGenericRepository<CaseMilestone> milestoneRepo,
        IGenericRepository<CaseCommunicationLog> communicationRepo,
        IGenericRepository<Document> docRepo,
        IGenericRepository<CaseNote> noteRepo)
    {
        _caseRepo = caseRepo;
        _complaintRepo = complaintRepo;
        _findingRepo = findingRepo;
        _milestoneRepo = milestoneRepo;
        _communicationRepo = communicationRepo;
        _docRepo = docRepo;
        _noteRepo = noteRepo;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<CaseListDto>>> GetCasesAsync(GetCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.CaseNumber.Value.Contains(request.Search) ||
                                         c.Complaint.ReferenceNumber.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Stage))
                query = query.Where(c => c.CurrentStage == request.Stage);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CaseListDto(
                    c.Id,
                    c.CaseNumber.Value,
                    c.ComplaintId,
                    c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer != null ? $"{c.Complaint.Taxpayer.FirstName} {c.Complaint.Taxpayer.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CasesRetrieved;
            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseRetrieveError;
        }
        return response;
    }

    public async Task<Response<PagedResult<CaseListDto>>> GetMyCasesAsync(GetMyCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => c.CaseNumber.Value.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Stage))
                query = query.Where(c => c.CurrentStage == request.Stage);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CaseListDto(
                    c.Id,
                    c.CaseNumber.Value,
                    c.ComplaintId,
                    c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer != null ? $"{c.Complaint.Taxpayer.FirstName} {c.Complaint.Taxpayer.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CasesRetrieved;
            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseRetrieveError;
        }
        return response;
    }

    public async Task<Response<PagedResult<CaseListDto>>> GetOverdueCasesAsync(GetOverdueCasesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<CaseListDto>>();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer)
                .Where(c => c.DueDate.HasValue && c.DueDate < now);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(c => c.DueDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CaseListDto(
                    c.Id,
                    c.CaseNumber.Value,
                    c.ComplaintId,
                    c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer != null ? $"{c.Complaint.Taxpayer.FirstName} {c.Complaint.Taxpayer.LastName}" : "Unknown",
                    c.Subject,
                    c.Priority,
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.DueDate,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.Data = new PagedResult<CaseListDto>(items, total, request.Page, request.PageSize);
            response.StatusCode = StatusCodes.Status200OK;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<QueueResultDto>> GetQueueAsync(GetQueueQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<QueueResultDto>();
        try
        {
            var query = _caseRepo.Query()
                .Include(c => c.Complaint)
                    .ThenInclude(co => co.Taxpayer)
                .Where(c => c.CurrentStage == request.QueueName);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new QueueItemDto(
                    c.Id,
                    c.Complaint.ReferenceNumber,
                    c.Subject,
                    "",
                    "",
                    c.Status.ToString(),
                    c.CurrentStage,
                    c.Complaint.Taxpayer != null ? $"{c.Complaint.Taxpayer.FirstName} {c.Complaint.Taxpayer.LastName}" : "Unknown",
                    c.AssignedOfficer != null ? c.AssignedOfficer.User.FirstName + " " + c.AssignedOfficer.User.LastName : "Unassigned",
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseQueueRetrieved;
            response.Data = new QueueResultDto(request.QueueName, items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseQueueError;
        }
        return response;
    }

    public async Task<Response<CaseDetailDto>> GetCaseByIdAsync(GetCaseByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CaseDetailDto>();
        try
        {
            var c = await _caseRepo.Query()
                .Include(x => x.Complaint).ThenInclude(co => co.Taxpayer)
                .Include(x => x.AssignedOfficer).ThenInclude(o => o!.User)
                .Include(x => x.Department)
                .Include(x => x.Findings)
                .Include(x => x.Recommendations)
                .Include(x => x.Milestones)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (c is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseRetrieved;
            response.Data = new CaseDetailDto(
                c.Id,
                c.CaseNumber.Value,
                c.Subject,
                c.Summary,
                c.Priority,
                c.Status.ToString(),
                c.CurrentStage,
                c.AssignedOfficer is null ? null : new CaseOfficerDto(c.AssignedOfficer.Id, $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}", c.AssignedOfficer.User.Email ?? string.Empty),
                c.Department is null ? null : new CaseDepartmentDto(c.Department.Id, c.Department.Name),
                c.DueDate,
                c.ClosedAt,
                c.Outcome,
                c.FindingsSummary,
                new CaseComplaintDto(c.Complaint.Id, c.Complaint.ReferenceNumber,
                    c.Complaint.Taxpayer is null ? null : new ComplaintTaxpayerDto(c.Complaint.Taxpayer.Id, $"{c.Complaint.Taxpayer.FirstName} {c.Complaint.Taxpayer.LastName}", c.Complaint.Taxpayer.Email.Value),
                    c.Complaint.TaxType, c.Complaint.TaxPeriod, c.Complaint.ComplaintCategory),
                c.Findings.Select(f => new FindingDto(f.Id, f.Description, f.CreatedAt)),
                c.Recommendations.Select(r => new RecommendationDto(r.Id, r.RecommendationText, r.ApprovedByUserId ?? Guid.Empty, r.CreatedAt)),
                c.Milestones.Select(m => new MilestoneDto(m.Id, m.Title, m.Description, m.CreatedAt)),
                c.StatusHistory.Select(h => new StatusHistoryDto(h.Id, h.OldStatus.ToString(), h.NewStatus.ToString(), h.ChangedByUserId, h.TransitionedAt))
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseGetError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseFindingDto>>> GetCaseFindingsAsync(GetCaseFindingsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseFindingDto>>();
        try
        {
            var findings = await _findingRepo.Query()
                .Where(f => f.CaseId == request.CaseId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new CaseFindingDto(f.Id, f.CaseId, f.Description, f.CreatedAt, f.CreatedByUserId))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingsRetrieved;
            response.Data = findings.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingsError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseMilestoneDto>>> GetCaseMilestonesAsync(GetCaseMilestonesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseMilestoneDto>>();
        try
        {
            var milestones = await _milestoneRepo.Query()
                .Where(m => m.CaseId == request.CaseId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new CaseMilestoneDto(m.Id, m.CaseId, m.Title, m.Description, m.TargetDate, m.CompletedAt, m.IsCompleted))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseMilestonesRetrieved;
            response.Data = milestones.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseMilestonesError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseCommunicationDto>>> GetCaseCommunicationsAsync(GetCaseCommunicationsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseCommunicationDto>>();
        try
        {
            var logs = await _communicationRepo.Query()
                .Where(l => l.CaseId == request.CaseId)
                .OrderByDescending(l => l.SentAt)
                .Select(l => new CaseCommunicationDto(l.Id, l.CaseId, l.Sender, l.Recipient, l.Direction.ToString(), l.Subject, l.Body, l.SentAt, l.Channel))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseCommsRetrieved;
            response.Data = logs.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseCommsError;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<CaseDocumentDto>>> GetCaseDocumentsAsync(GetCaseDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CaseDocumentDto>>();
        try
        {
            var documents = await _docRepo.Query()
                .Where(d => d.EntityId == request.CaseId && d.EntityType == DocumentEntityType.Case)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new CaseDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentsRetrieved;
            response.Data = documents.AsReadOnly();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocsError;
        }
        return response;
    }

    public async Task<Response<TrackComplaintResponse>> TrackComplaintAsync(TrackComplaintQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TrackComplaintResponse>();
        try
        {
            var complaint = await _complaintRepo.Query()
                .FirstOrDefaultAsync(c => c.ReferenceNumber == request.TrackingNumber, cancellationToken);

            if (complaint is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.ComplaintTrackNotFound;
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.ComplaintTracked;
            response.Data = new TrackComplaintResponse(
                complaint.ReferenceNumber,
                complaint.Status.ToString(),
                complaint.CurrentStage,
                complaint.Description,
                complaint.CreatedAt,
                complaint.LastModifiedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ComplaintTrackError;
        }
        return response;
    }

    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<SubmitPublicCaseResponse>> SubmitPublicCaseAsync(SubmitPublicCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SubmitPublicCaseResponse>();
        try
        {
            var refNumber = $"TOC-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var complaint = Complaint.Create(
                Guid.Empty,
                "General",
                "N/A",
                "Public",
                request.Description[..Math.Min(100, request.Description.Length)],
                request.Description,
                refNumber
            );
            complaint.Submit();
            complaint.UpdateStage("input");

            await _complaintRepo.AddAsync(complaint);
            await _complaintRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseCreated;
            response.Data = new SubmitPublicCaseResponse(complaint.Id, refNumber);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseSubmitError;
        }
        return response;
    }

    public async Task<Response<AddCaseNoteResponse>> AddCaseNoteAsync(AddCaseNoteCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AddCaseNoteResponse>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            var note = new CaseNote
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                Content = request.Text,
                IsInternal = !request.IsExternal,
                CreatedAt = DateTime.UtcNow
            };

            await _noteRepo.AddAsync(note);
            await _noteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.NoteAdded;
            response.Data = new AddCaseNoteResponse(note.Id, note.Content, !note.IsInternal, note.CreatedAt);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseNoteAddError;
        }
        return response;
    }

    public async Task<Response<Guid>> AddCaseFindingAsync(AddCaseFindingCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var finding = new CaseFinding
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _findingRepo.AddAsync(finding);
            await _findingRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingAdded;
            response.Data = finding.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingAddError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateCaseFindingAsync(UpdateCaseFindingCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var finding = await _findingRepo.Query()
                .FirstOrDefaultAsync(f => f.Id == request.FindingId && f.CaseId == request.CaseId, cancellationToken);

            if (finding is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseFindingNotFound;
                return response;
            }

            finding.Description = request.Description;
            await _findingRepo.UpdateAsync(finding);
            await _findingRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseFindingUpdated;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseFindingUpdateError;
        }
        return response;
    }

    public async Task<Response<object?>> AssignCaseAsync(AssignCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            caseEntity.Assign(request.OfficerId, Guid.Empty);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseAssigned;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseAssignError;
        }
        return response;
    }

    public async Task<Response<object?>> TransitionCaseAsync(TransitionCaseCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            caseEntity.UpdateStatus(caseEntity.Status, request.TargetStage, Guid.Empty);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = string.Format(Constants.Messages.CaseUpdated);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseTransitionError;
        }
        return response;
    }

    public async Task<Response<PostRecommendationResponse>> PostRecommendationAsync(PostRecommendationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PostRecommendationResponse>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            var rec = new CaseRecommendation
            {
                Id = Guid.NewGuid(),
                CaseId = request.CaseId,
                RecommendationText = request.RecommendationText,
                CreatedAt = DateTime.UtcNow
            };

            caseEntity.Recommendations.Add(rec);
            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseRecommendationPosted;
            response.Data = new PostRecommendationResponse(rec.Id, rec.RecommendationText, rec.CreatedAt);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseRecommendationError;
        }
        return response;
    }

    public async Task<Response<object?>> ApproveClosureAsync(ApproveClosureCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var caseEntity = await _caseRepo.GetByIdAsync(request.CaseId);
            if (caseEntity is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            if (request.Rationale.Length < 100)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.CaseCeApprovalRationaleShort;
                return response;
            }

            if (request.Approve)
            {
                caseEntity.Close("Approved for closure", request.Rationale, Guid.Empty);
            }

            await _caseRepo.UpdateAsync(caseEntity);
            await _caseRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = request.Approve ? "Case closure approved." : "Case closure rejected.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseClosureApprovalError;
        }
        return response;
    }

    public async Task<Response<object?>> CompleteMilestoneAsync(CompleteMilestoneCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var milestone = await _milestoneRepo.Query()
                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId && m.CaseId == request.CaseId, cancellationToken);

            if (milestone is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseMilestoneNotFound;
                return response;
            }

            milestone.IsCompleted = true;
            milestone.CompletedAt = DateTimeOffset.UtcNow;
            await _milestoneRepo.UpdateAsync(milestone);
            await _milestoneRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.CaseMilestoneCompleted;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseMilestoneCompleteError;
        }
        return response;
    }

    public async Task<Response<Guid>> UploadCaseDocumentAsync(UploadCaseDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var docId = Guid.NewGuid();
            var doc = new Document
            {
                Id = docId,
                EntityId = request.CaseId,
                EntityType = DocumentEntityType.Case,
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                CreatedAt = DateTime.UtcNow
            };

            await _docRepo.AddAsync(doc);
            await _docRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentUploaded;
            response.Data = docId;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocUploadError;
        }
        return response;
    }
}
