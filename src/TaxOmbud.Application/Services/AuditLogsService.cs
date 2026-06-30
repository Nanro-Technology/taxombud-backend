using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.AuditLogs.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
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
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Services;

public class AuditLogsService : IAuditLogsService
{
    private readonly IApplicationDbContext _context;

    public AuditLogsService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<AuditLogDetailDto>> GetAuditLogByIdAsync(GetAuditLogByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<AuditLogDetailDto>();
        try
        {
            var log = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (log == null)
            {
                response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound;
                response.Message = "Audit log entry not found.";
                return response;
            }

            var dto = new AuditLogDetailDto(
                log.Id,
                log.EntityType,
                log.EntityId,
                log.Action,
                log.UserId,
                log.ImpersonatorUserId,
                log.OldValues,
                log.NewValues,
                log.IPAddress,
                log.UserAgent,
                log.CreatedAt
            );

            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "Audit log retrieved successfully.";
            response.Data = dto;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the audit log.";
            return response;
        }
    }

    public async Task<Response<PagedResult<AuditLogDto>>> GetAuditLogsAsync(GetAuditLogsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<AuditLogDto>>();
        try
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.EntityType))
                query = query.Where(l => l.EntityType == request.EntityType);

            if (request.EntityId.HasValue)
                query = query.Where(l => l.EntityId == request.EntityId.Value);

            if (request.UserId.HasValue)
                query = query.Where(l => l.UserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(l => l.Action == request.Action);

            if (request.From.HasValue)
                query = query.Where(l => l.CreatedAt >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(l => l.CreatedAt <= request.To.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new AuditLogDto(
                    l.Id,
                    l.EntityType,
                    l.EntityId,
                    l.Action,
                    l.UserId,
                    l.ImpersonatorUserId,
                    l.OldValues,
                    l.NewValues,
                    l.IPAddress,
                    l.UserAgent,
                    l.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<AuditLogDto>(items, total, request.Page, request.PageSize);
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
            response.Message = "Audit logs retrieved successfully.";
            response.Data = pagedResult;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving audit logs.";
            return response;
        }
    }

}