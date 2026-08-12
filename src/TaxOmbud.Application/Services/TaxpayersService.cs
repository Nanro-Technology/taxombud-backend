using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Taxpayers.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Taxpayers;
using Microsoft.AspNetCore.Identity;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Application.Services;

public class TaxpayersService : ITaxpayersService
{
    private readonly IGenericRepository<TaxpayerProfile> _taxpayerRepo;
    private readonly IGenericRepository<Complaint> _complaintRepo;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _dbContext;

    public TaxpayersService(
        IGenericRepository<TaxpayerProfile> taxpayerRepo,
        IGenericRepository<Complaint> complaintRepo,
        ICurrentUser currentUser,
        UserManager<User> userManager,
        IApplicationDbContext dbContext)
    {
        _taxpayerRepo = taxpayerRepo;
        _complaintRepo = complaintRepo;
        _currentUser = currentUser;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<TaxpayerListDto>>> GetTaxpayersAsync(GetTaxpayersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<TaxpayerListDto>>();
        try
        {
            var query = _taxpayerRepo.Query()
                .Include(t => t.User)
                .Include(t => t.Account)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(t =>
                    t.User!.FirstName!.Contains(request.Search) ||
                    t.User!.LastName!.Contains(request.Search) ||
                    t.User!.Email!.Contains(request.Search) ||
                    (t.TinNumber != null && t.TinNumber.Contains(request.Search)));

            if (!string.IsNullOrWhiteSpace(request.Type))
                query = query.Where(t => t.TaxpayerType.ToString() == request.Type);

            if (request.IsVerified.HasValue)
                query = query.Where(t => t.IsVerified == request.IsVerified.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TaxpayerListDto(
                    t.Id, t.UserId, $"{t.User.FirstName} {t.User.LastName}",
                    t.User!.Email ?? string.Empty, t.User.Phone, t.TaxpayerType.ToString(),
                    t.TinNumber, t.Nin, t.Bvn, t.CompanyName, t.RcNumber, t.IsVerified, t.CreatedAt,
                    t.Account != null ? t.Account.Name : "-"
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayers retrieved successfully.";
            response.Data = new PagedResult<TaxpayerListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving taxpayers.";
        }
        return response;
    }

    public async Task<Response<TaxpayerDetailDto>> GetTaxpayerByIdAsync(GetTaxpayerByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TaxpayerDetailDto>();
        try
        {
            var t = await _taxpayerRepo.Query()
                .Include(x => x.User)
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (t is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer retrieved successfully.";
            response.Data = MapToDetailDto(t);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the taxpayer.";
        }
        return response;
    }

    public async Task<Response<TaxpayerDetailDto>> GetTaxpayerByTinAsync(GetTaxpayerByTinQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TaxpayerDetailDto>();
        try
        {
            var t = await _taxpayerRepo.Query()
                .Include(x => x.User)
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.TinNumber == request.Tin, cancellationToken);

            if (t is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "No taxpayer found with the given TIN.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer retrieved successfully.";
            response.Data = MapToDetailDto(t);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the taxpayer.";
        }
        return response;
    }

    public async Task<Response<TaxpayerDetailDto>> GetCurrentTaxpayerAsync(GetCurrentTaxpayerQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TaxpayerDetailDto>();
        try
        {
            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "User is not authenticated.";
                return response;
            }

            var t = await _taxpayerRepo.Query()
                .Include(x => x.User)
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

            if (t is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer profile not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer profile retrieved successfully.";
            response.Data = MapToDetailDto(t);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the taxpayer profile.";
        }
        return response;
    }

    public async Task<Response<PagedResult<ComplaintSummaryDto>>> GetTaxpayerComplaintsAsync(GetTaxpayerComplaintsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ComplaintSummaryDto>>();
        try
        {
            var query = _complaintRepo.Query()
                .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .Where(c => c.TaxpayerId == request.TaxpayerId);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ComplaintSummaryDto(
                    c.Id, c.ReferenceNumber, c.Subject, c.TaxType, c.TaxPeriod, c.ComplaintCategory,
                    c.Status.ToString(), c.CurrentStage, c.Priority,
                    c.TaxpayerId, c.Taxpayer != null && c.Taxpayer.User != null ? $"{c.Taxpayer.User.FirstName} {c.Taxpayer.User.LastName}" : null,
                    c.AssignedOfficerId, c.AssignedOfficer != null ? $"{c.AssignedOfficer.User.FirstName} {c.AssignedOfficer.User.LastName}" : null,
                    c.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer complaints retrieved successfully.";
            response.Data = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving taxpayer complaints.";
        }
        return response;
    }

    public async Task<Response<NinVerificationResponseDto>> VerifyNinAsync(VerifyNinQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<NinVerificationResponseDto>();
        try
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "NIN verification stub — integrate with NIMC API.";
            response.Data = new NinVerificationResponseDto(false, request.Nin, "", "", "", "", "");
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred during NIN verification.";
        }
        return response;
    }

    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<object?>> CreateTaxpayerAsync(CreateTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
            if (existingUser is not null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"An account with email '{request.Email}' already exists.";
                return response;
            }

            var emailVo = new Email(emailNormalized);
            var user = User.Create(
                request.FirstName,
                request.LastName,
                emailVo,
                request.Phone,
                UserType.RegisteredTaxpayer
            );
            user.AltPhone = request.AltPhone;

            // Default password for administrative creation
            var createResult = await _userManager.CreateAsync(user, "Taxpayer@123");
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = errors;
                return response;
            }

            // Create TaxpayerProfile
            var profile = TaxpayerProfile.Create(user.Id, TaxpayerType.Individual.ToString());
            profile.Gender = request.Gender;
            profile.Nin = request.Nin;
            profile.Bvn = request.Bvn;
            profile.TinNumber = request.TinNumber;
            profile.CompanyName = request.CompanyName;
            profile.RcNumber = request.RcNumber;
            profile.Address = request.Address;
            profile.City = request.City;
            profile.State = request.State;
            profile.Country = request.Country ?? "Nigeria";

            // Resolve Zonal Office Account
            if (!string.IsNullOrWhiteSpace(request.Account) && request.Account != "-")
            {
                var accountName = request.Account.Trim();
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(acc => acc.Name == accountName, cancellationToken);
                profile.AccountId = account?.Id;
            }

            await _taxpayerRepo.AddAsync(profile);
            await _taxpayerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer created successfully.";
            response.Data = new { Id = profile.Id };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateTaxpayerAsync(UpdateTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var taxpayer = await _taxpayerRepo.Query()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);

            if (taxpayer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            if (taxpayer.User is not null)
            {
                taxpayer.User.UpdateProfile(request.FirstName, request.LastName, request.Phone, taxpayer.User.JobTitle);
                taxpayer.User.AltPhone = request.AltPhone;
            }

            if (request.Address is not null) taxpayer.Address = request.Address;
            if (request.City is not null) taxpayer.City = request.City;
            if (request.State is not null) taxpayer.State = request.State;
            if (request.Country is not null) taxpayer.Country = request.Country;
            if (request.CompanyName is not null) taxpayer.CompanyName = request.CompanyName;
            if (request.RcNumber is not null) taxpayer.RcNumber = request.RcNumber;
            if (request.TinNumber is not null) taxpayer.TinNumber = request.TinNumber;
            if (request.Nin is not null) taxpayer.Nin = request.Nin;
            if (request.Bvn is not null) taxpayer.Bvn = request.Bvn;
            if (request.Gender is not null) taxpayer.Gender = request.Gender;
            if (request.DateOfBirth is not null) taxpayer.DateOfBirth = request.DateOfBirth;

            // Resolve Account
            if (!string.IsNullOrWhiteSpace(request.Account))
            {
                if (request.Account == "-")
                {
                    taxpayer.AccountId = null;
                }
                else
                {
                    var accountName = request.Account.Trim();
                    var account = await _dbContext.Accounts.FirstOrDefaultAsync(acc => acc.Name == accountName, cancellationToken);
                    taxpayer.AccountId = account?.Id;
                }
            }

            taxpayer.LastModifiedAt = DateTime.UtcNow;
            await _taxpayerRepo.UpdateAsync(taxpayer);
            await _taxpayerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer updated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the taxpayer.";
        }
        return response;
    }

    public async Task<Response<object?>> VerifyTaxpayerAsync(VerifyTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var taxpayer = await _taxpayerRepo.GetByIdAsync(request.TaxpayerId);
            if (taxpayer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            taxpayer.IsVerified = true;
            taxpayer.LastModifiedAt = DateTime.UtcNow;
            await _taxpayerRepo.UpdateAsync(taxpayer);
            await _taxpayerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer verified successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while verifying the taxpayer.";
        }
        return response;
    }

    public async Task<Response<object?>> DeactivateTaxpayerAsync(DeactivateTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var taxpayer = await _taxpayerRepo.Query()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);

            if (taxpayer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            if (taxpayer.User is not null)
                taxpayer.User.Deactivate();

            taxpayer.LastModifiedAt = DateTime.UtcNow;
            await _taxpayerRepo.UpdateAsync(taxpayer);
            await _taxpayerRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Taxpayer deactivated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deactivating the taxpayer.";
        }
        return response;
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private static TaxpayerDetailDto MapToDetailDto(TaxpayerProfile t) => new(
        t.Id, t.UserId, t.User?.FirstName ?? string.Empty, t.User?.LastName ?? string.Empty,
        t.User != null ? $"{t.User.FirstName} {t.User.LastName}" : string.Empty,
        t.User?.Email ?? string.Empty, t.User?.Phone, t.TaxpayerType.ToString(),
        t.TinNumber, t.Nin, t.Bvn, t.Gender, t.DateOfBirth,
        t.CompanyName, t.RcNumber, t.Address, t.City, t.State,
        t.IsVerified, t.CreatedAt, t.LastModifiedAt,
        t.Account != null ? t.Account.Name : "-"
    );
}
