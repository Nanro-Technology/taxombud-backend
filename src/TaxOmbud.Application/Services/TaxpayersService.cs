using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Taxpayers.DTOs;
using TaxOmbud.Application.Complaints.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Application.Services;

public class TaxpayersService : ITaxpayersService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public TaxpayersService(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<TaxpayerListDto>>> GetTaxpayersAsync(GetTaxpayersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<TaxpayerListDto>>();
        try
        {
            var query = _context.TaxpayerProfiles
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(t =>
                    t.User.FirstName.Contains(request.Search) ||
                    t.User.LastName.Contains(request.Search) ||
                    t.User.Email.Contains(request.Search) ||
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
                    t.Id,
                    t.UserId,
                    $"{t.User.FirstName} {t.User.LastName}",
                    t.User.Email,
                    t.User.Phone,
                    t.TaxpayerType.ToString(),
                    t.TinNumber,
                    t.Nin,
                    t.Bvn,
                    t.CompanyName,
                    t.RcNumber,
                    t.IsVerified,
                    t.CreatedAt
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
            var t = await _context.TaxpayerProfiles
                .Include(x => x.User)
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
            var t = await _context.TaxpayerProfiles
                .Include(x => x.User)
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

            var t = await _context.TaxpayerProfiles
                .Include(x => x.User)
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
            var query = _context.Complaints
                .Include(c => c.Taxpayer)
                .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
                .Where(c => c.TaxpayerId == request.TaxpayerId);

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

    public async Task<Response<object?>> UpdateTaxpayerAsync(UpdateTaxpayerCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var taxpayer = await _context.TaxpayerProfiles
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);

            if (taxpayer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            if (request.Phone is not null && taxpayer.User is not null)
            {
                taxpayer.User.UpdateProfile(taxpayer.User.FirstName, taxpayer.User.LastName, request.Phone, taxpayer.User.JobTitle);
            }
            if (request.Address is not null) taxpayer.Address = request.Address;
            if (request.City is not null) taxpayer.City = request.City;
            if (request.State is not null) taxpayer.State = request.State;
            if (request.CompanyName is not null) taxpayer.CompanyName = request.CompanyName;
            if (request.RcNumber is not null) taxpayer.RcNumber = request.RcNumber;

            taxpayer.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

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
            var taxpayer = await _context.TaxpayerProfiles.FindAsync(new object[] { request.TaxpayerId }, cancellationToken);
            if (taxpayer is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Taxpayer not found.";
                return response;
            }

            taxpayer.IsVerified = true;
            taxpayer.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

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
            var taxpayer = await _context.TaxpayerProfiles
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
                taxpayer.User.Deactivate();
            }
            taxpayer.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

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

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static TaxpayerDetailDto MapToDetailDto(TaxpayerProfile t) => new(
        t.Id,
        t.UserId,
        t.User?.FirstName ?? string.Empty,
        t.User?.LastName ?? string.Empty,
        t.User != null ? $"{t.User.FirstName} {t.User.LastName}" : string.Empty,
        t.User?.Email ?? string.Empty,
        t.User?.Phone,
        t.TaxpayerType.ToString(),
        t.TinNumber,
        t.Nin,
        t.Bvn,
        t.Gender,
        t.DateOfBirth,
        t.CompanyName,
        t.RcNumber,
        t.Address,
        t.City,
        t.State,
        t.IsVerified,
        t.CreatedAt,
        t.UpdatedAt
    );
}
