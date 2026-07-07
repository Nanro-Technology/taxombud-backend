using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Appeals.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Application.Services;

public class AppealsService : IAppealsService
{
    private readonly IGenericRepository<Appeal> _appealRepo;
    private readonly IGenericRepository<Case> _caseRepo;
    private readonly IGenericRepository<Document> _docRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _storage;

    public AppealsService(
        IGenericRepository<Appeal> appealRepo,
        IGenericRepository<Case> caseRepo,
        IGenericRepository<Document> docRepo,
        ICurrentUser currentUser,
        IFileStorageService storage
    )
    {
        _appealRepo = appealRepo;
        _caseRepo = caseRepo;
        _docRepo = docRepo;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<Response<FileAppealResponse>> FileAppealAsync(FileAppealCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<FileAppealResponse>();
        try
        {
            var kase = await _caseRepo.FindAsync(c => c.Id == request.CaseId);
            if (kase == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.CaseNotFound;
                return response;
            }

            if (kase.Status != CaseStatus.Closed)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = Constants.Messages.AppealClosedCaseOnly;
                return response;
            }

            var actorUserId = _currentUser.UserId ?? Guid.Empty;
            var appeal = new Appeal(request.CaseId, request.Reason);
            appeal.Submit(actorUserId);

            await _appealRepo.AddAsync(appeal);
            await _appealRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.AppealSubmitted;
            response.Data = new FileAppealResponse(
                appeal.Id,
                appeal.CaseId,
                appeal.Reason,
                appeal.CreatedAt
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.AppealFileError;
            return response;
        }
    }

    public async Task<Response<object?>> ReviewAppealAsync(ReviewAppealCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var appeal = await _appealRepo.FindAsync(a => a.Id == request.AppealId);
            if (appeal == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AppealNotFound;
                return response;
            }

            var actorUserId = _currentUser.UserId ?? Guid.Empty;

            if (request.Action.ToLowerInvariant() == "uphold")
            {
                appeal.Uphold(actorUserId, request.Notes);
            }
            else if (request.Action.ToLowerInvariant() == "dismiss")
            {
                appeal.Dismiss(actorUserId, request.Notes);
            }
            else
            {
                appeal.Review(actorUserId, request.Notes);
            }

            await _appealRepo.UpdateAsync(appeal);
            await _appealRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.AppealStatusUpdated;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.AppealReviewError;
            return response;
        }
    }

    public async Task<Response<Guid>> UploadAppealDocumentAsync(UploadAppealDocumentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var exists = await _appealRepo.ExistsAsync(a => a.Id == request.AppealId);

            if (!exists)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AppealNotFound;
                return response;
            }

            await using var stream = request.File.OpenReadStream();
            var path = await _storage.StoreAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                cancellationToken);

            var doc = new Document
            {
                Id = Guid.NewGuid(),
                FileName = request.File.FileName,
                FilePath = path,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                EntityType = DocumentEntityType.Appeal,
                EntityId = request.AppealId,
                CreatedAt = DateTime.UtcNow
            };

            await _docRepo.AddAsync(doc);
            await _docRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentUploaded;
            response.Data = doc.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocUploadError;
            return response;
        }
    }

    public async Task<Response<AppealDetailDto>> GetAppealByIdAsync(GetAppealByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AppealDetailDto>();
        try
        {
            var appeal = await _appealRepo.Query()
                .Include(a => a.Case)
                .Include(a => a.StatusHistory)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (appeal == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AppealNotFound;
                return response;
            }

            var dto = new AppealDetailDto(
                appeal.Id,
                appeal.CaseId,
                appeal.Case!.CaseNumber.Value,
                appeal.Case.Subject,
                appeal.Reason,
                appeal.Status.ToString(),
                appeal.ReviewedByUserId,
                appeal.ReviewNote,
                appeal.ReviewedAt,
                appeal.CreatedAt,
                appeal.StatusHistory.Select(h => new AppealStatusHistoryDto(
                    h.Id,
                    h.OldStatus.ToString(),
                    h.NewStatus.ToString(),
                    h.Reason,
                    h.ChangedByUserId,
                    h.TransitionedAt
                ))
            );

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.AppealRetrieved;
            response.Data = dto;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.AppealGetError;
            return response;
        }
    }

    public async Task<Response<IReadOnlyList<AppealDocumentDto>>> GetAppealDocumentsAsync(GetAppealDocumentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<AppealDocumentDto>>();
        try
        {
            var exists = await _appealRepo.ExistsAsync(a => a.Id == request.AppealId);

            if (!exists)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = Constants.Messages.AppealNotFound;
                return response;
            }

            var documents = await _docRepo.Query()
                .AsNoTracking()
                .Where(d => d.EntityType == DocumentEntityType.Appeal && d.EntityId == request.AppealId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new AppealDocumentDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.DocumentsRetrieved;
            response.Data = documents.AsReadOnly();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.CaseDocsError;
            return response;
        }
    }

    public async Task<Response<PagedResult<AppealListDto>>> GetAppealsAsync(GetAppealsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<AppealListDto>>();
        try
        {
            var query = _appealRepo.Query()
                .Include(a => a.Case)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppealStatus>(request.Status, true, out var appealStatus))
            {
                query = query.Where(a => a.Status == appealStatus);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AppealListDto(
                    a.Id,
                    a.CaseId,
                    a.Case!.CaseNumber.Value,
                    a.Case.Subject,
                    a.Reason,
                    a.Status.ToString(),
                    a.ReviewedByUserId,
                    a.ReviewedAt,
                    a.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = Constants.Messages.AppealsRetrieved;
            response.Data = new PagedResult<AppealListDto>(items.AsReadOnly(), total, request.Page, request.PageSize);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.AppealRetrieveError;
            return response;
        }
    }
}
