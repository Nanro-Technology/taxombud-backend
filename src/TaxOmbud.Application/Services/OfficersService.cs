using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Officers.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Officers;

namespace TaxOmbud.Application.Services;

public class OfficersService : IOfficersService
{
    private readonly IGenericRepository<OfficerProfile> _officerRepo;
    private readonly IGenericRepository<OfficerCaseload> _caseloadRepo;

    public OfficersService(
        IGenericRepository<OfficerProfile> officerRepo,
        IGenericRepository<OfficerCaseload> caseloadRepo)
    {
        _officerRepo = officerRepo;
        _caseloadRepo = caseloadRepo;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<OfficerListDto>>> GetOfficersAsync(GetOfficersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<OfficerListDto>>();
        try
        {
            var query = _officerRepo.Query()
                .Include(o => o.User)
                .Include(o => o.User.Department)
                .AsQueryable();

            if (request.DepartmentId.HasValue)
                query = query.Where(o => o.User.DepartmentId == request.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(o =>
                    o.User!.FirstName!.Contains(request.Search) ||
                    o.User!.LastName!.Contains(request.Search) ||
                    o.User!.Email!.Contains(request.Search));

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(o => o.User.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OfficerListDto(
                    o.Id, o.UserId, $"{o.User.FirstName} {o.User.LastName}",
                    o.User!.Email ?? string.Empty, o.User.Phone, o.User.JobTitle,
                    o.User.Department == null ? null : new OfficerDepartmentDto(o.User.Department.Id, o.User.Department.Name),
                    o.MaxCaseload, o.CurrentCaseload, o.IsAvailable, o.EmployeeNumber, o.Specialisation, o.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officers retrieved successfully.";
            response.Data = new PagedResult<OfficerListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving officers.";
        }
        return response;
    }

    public async Task<Response<PagedResult<OfficerListDto>>> GetAvailableOfficersAsync(GetAvailableOfficersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<OfficerListDto>>();
        try
        {
            var query = _officerRepo.Query()
                .Include(o => o.User)
                .Include(o => o.User.Department)
                .Where(o => o.IsAvailable && o.CurrentCaseload < o.MaxCaseload);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(o => o.CurrentCaseload)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OfficerListDto(
                    o.Id, o.UserId, $"{o.User.FirstName} {o.User.LastName}",
                    o.User!.Email ?? string.Empty, o.User.Phone, o.User.JobTitle,
                    o.User.Department == null ? null : new OfficerDepartmentDto(o.User.Department.Id, o.User.Department.Name),
                    o.MaxCaseload, o.CurrentCaseload, o.IsAvailable, o.EmployeeNumber, o.Specialisation, o.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Available officers retrieved successfully.";
            response.Data = new PagedResult<OfficerListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving available officers.";
        }
        return response;
    }

    public async Task<Response<OfficerDetailDto>> GetOfficerByIdAsync(GetOfficerByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<OfficerDetailDto>();
        try
        {
            var o = await _officerRepo.Query()
                .Include(x => x.User)
                .Include(x => x.User.Department)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (o is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Officer not found.";
                return response;
            }

            var activeCaseloadCount = await _caseloadRepo.CountAsync(
                c => c.OfficerProfileId == o.Id && c.IsActive);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officer retrieved successfully.";
            response.Data = new OfficerDetailDto(
                o.Id, o.UserId, $"{o.User.FirstName} {o.User.LastName}",
                o.User.Email, o.User.Phone, o.User.JobTitle,
                o.User.Department == null ? null : new OfficerDepartmentDto(o.User.Department.Id, o.User.Department.Name),
                o.MaxCaseload, o.CurrentCaseload, o.IsAvailable, o.EmployeeNumber, o.Specialisation,
                activeCaseloadCount, o.CreatedAt
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the officer.";
        }
        return response;
    }

    public async Task<Response<OfficerCaseloadsDto>> GetOfficerCaseloadsAsync(GetOfficerCaseloadsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<OfficerCaseloadsDto>();
        try
        {
            var query = _caseloadRepo.Query()
                .Where(c => c.OfficerProfileId == request.OfficerId);

            if (request.ActiveOnly == true)
                query = query.Where(c => c.IsActive);

            var caseloads = await query
                .OrderByDescending(c => c.AssignedAt)
                .Select(c => new CaseloadDto(c.Id, c.CaseId, c.IsActive, c.AssignedAt, c.CompletedAt))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Caseloads retrieved successfully.";
            response.Data = new OfficerCaseloadsDto(request.OfficerId, caseloads);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving caseloads.";
        }
        return response;
    }

    public async Task<Response<OfficerPerformanceDto>> GetOfficerPerformanceAsync(GetOfficerPerformanceQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<OfficerPerformanceDto>();
        try
        {
            var officer = await _officerRepo.Query()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == request.OfficerId, cancellationToken);

            if (officer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Officer not found.";
                return response;
            }

            var completedCaseloads = await _caseloadRepo.FindAllAsync(
                c => c.OfficerProfileId == request.OfficerId && !c.IsActive && c.CompletedAt.HasValue);

            var caseloadList = completedCaseloads.ToList();
            var casesHandled = caseloadList.Count;
            var avgResolutionDays = casesHandled > 0
                ? caseloadList.Average(c => (c.CompletedAt!.Value - c.AssignedAt).TotalDays)
                : 0;

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officer performance retrieved successfully.";
            response.Data = new OfficerPerformanceDto(
                officer.Id, $"{officer.User.FirstName} {officer.User.LastName}",
                casesHandled, Math.Round(avgResolutionDays, 2)
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving officer performance.";
        }
        return response;
    }

    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<CreatedOfficerResponse>> CreateOfficerProfileAsync(CreateOfficerProfileCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreatedOfficerResponse>();
        try
        {
            if (await _officerRepo.ExistsAsync(o => o.UserId == request.UserId))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "An officer profile already exists for this user.";
                return response;
            }

            var profile = new OfficerProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                MaxCaseload = request.MaxCaseload,
                CurrentCaseload = 0,
                IsAvailable = true,
                EmployeeNumber = request.EmployeeNumber,
                Specialisation = request.Specialisation,
                CreatedAt = DateTime.UtcNow
            };

            await _officerRepo.AddAsync(profile);
            await _officerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officer profile created successfully.";
            response.Data = new CreatedOfficerResponse(profile.Id, profile.UserId, profile.MaxCaseload, profile.EmployeeNumber, profile.Specialisation);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the officer profile.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateOfficerProfileAsync(UpdateOfficerProfileCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var profile = await _officerRepo.FindAsync(o => o.Id == request.Id);
            if (profile is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Officer profile not found.";
                return response;
            }

            profile.MaxCaseload = request.MaxCaseload;
            profile.IsAvailable = request.IsAvailable;
            if (request.EmployeeNumber is not null) profile.EmployeeNumber = request.EmployeeNumber;
            if (request.Specialisation is not null) profile.Specialisation = request.Specialisation;
            profile.LastModifiedAt = DateTime.UtcNow;

            await _officerRepo.UpdateAsync(profile);
            await _officerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Officer profile updated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the officer profile.";
        }
        return response;
    }
}
