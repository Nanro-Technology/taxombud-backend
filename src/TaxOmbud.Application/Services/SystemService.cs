using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.System.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Services;

public class SystemService : ISystemService
{
    private readonly IGenericRepository<Announcement> _announcementRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<AuditLog> _auditLogRepo;
    private readonly IGenericRepository<FeatureFlag> _featureFlagRepo;
    private readonly IGenericRepository<SystemSetting> _settingRepo;
    private readonly ICurrentUser _currentUser;
    private readonly ITokenService _tokenService;

    public SystemService(
        IGenericRepository<Announcement> announcementRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<AuditLog> auditLogRepo,
        IGenericRepository<FeatureFlag> featureFlagRepo,
        IGenericRepository<SystemSetting> settingRepo,
        ICurrentUser currentUser,
        ITokenService tokenService
    )
    {
        _announcementRepo = announcementRepo;
        _userRepo = userRepo;
        _auditLogRepo = auditLogRepo;
        _featureFlagRepo = featureFlagRepo;
        _settingRepo = settingRepo;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    public async Task<Response<Guid>> CreateAnnouncementAsync(CreateAnnouncementCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Message = request.Message,
            Scope = request.Scope,
            CreatedAt = DateTime.UtcNow
        };
        await _announcementRepo.AddAsync(entity);
        await _announcementRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<ImpersonationResponseDto>> ImpersonateUserAsync(ImpersonateUserCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<ImpersonationResponseDto>();
        var adminUserId = _currentUser.UserId ?? Guid.Empty;
        if (adminUserId == request.UserId)
            return new Response<ImpersonationResponseDto> { StatusCode = StatusCodes.Status400BadRequest, Message = "Cannot impersonate yourself." };
        try
        {
            var targetUser = await _userRepo.Query()
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (targetUser == null)
                return new Response<ImpersonationResponseDto> { StatusCode = StatusCodes.Status404NotFound, Message = "Target user not found." };

            if (targetUser.Role?.Name == RoleConstants.SuperAdmin)
            {
                return new Response<ImpersonationResponseDto> { StatusCode = StatusCodes.Status400BadRequest, Message = "Cannot impersonate another Super Admin without dual-control approval." };
            }

            var roles = targetUser.Role is not null
                ? new List<string> { targetUser.Role.Name }
                : new List<string>();

            var permissions = targetUser.Role?.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => $"{rp.Permission.Module}:{rp.Permission.Action}")
                .Distinct()
                .ToList() ?? new List<string>();

            var token = _tokenService.GenerateAccessToken(targetUser.Id, targetUser.Email ?? string.Empty, targetUser.UserType, roles, permissions);

            var audit = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = targetUser.Id,
                ImpersonatorUserId = adminUserId,
                Action = "ImpersonationStart",
                EntityType = "Users",
                EntityId = targetUser.Id,
                NewValues = $"Admin started impersonation of user {targetUser.Email}",
                IPAddress = _currentUser.IpAddress,
                UserAgent = _currentUser.UserAgent,
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepo.AddAsync(audit);
            await _auditLogRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new ImpersonationResponseDto(
                $"Now impersonating {targetUser.FullName}.",
                token,
                targetUser.Id,
                adminUserId
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<StopImpersonationResponseDto>> StopImpersonationAsync(StopImpersonationCommand request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = "ImpersonationEnd",
            EntityType = "Users",
            EntityId = currentUserId,
            OldValues = $"User: {currentUserId}",
            NewValues = "Impersonation Session Terminated",
            IPAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            CreatedAt = DateTime.UtcNow
        };
        await _auditLogRepo.AddAsync(audit);
        await _auditLogRepo.SaveAsync();

        return new Response<StopImpersonationResponseDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = new StopImpersonationResponseDto("Impersonation session terminated successfully.") };
    }

    public async Task<Response<object?>> ToggleFeatureFlagAsync(ToggleFeatureFlagCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        var flag = await _featureFlagRepo.FindAsync(f => f.Id == request.Id);
        if (flag == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Feature flag not found." };
        try
        {
            flag.IsEnabled = !flag.IsEnabled;
            await _featureFlagRepo.UpdateAsync(flag);
            await _featureFlagRepo.SaveAsync();

            return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<object?>> UpdateSettingAsync(UpdateSettingCommand request, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepo.FindAsync(s => s.Key == request.Key);
        if (setting == null)
        {
            setting = new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Value = request.Value,
                Description = request.Description
            };
            await _settingRepo.AddAsync(setting);
        }
        else
        {
            setting.Value = request.Value;
            if (request.Description != null)
                setting.Description = request.Description;
            await _settingRepo.UpdateAsync(setting);
        }

        await _settingRepo.SaveAsync();

        return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
    }

    public async Task<Response<object?>> DeleteSettingAsync(DeleteSettingCommand request, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepo.FindAsync(s => s.Key == request.Key);
        if (setting == null)
        {
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Setting not found." };
        }

        await _settingRepo.RemoveAsync(setting);
        await _settingRepo.SaveAsync();

        return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
    }

    public async Task<Response<PagedResult<AuditLog>>> GetAdminAuditLogsAsync(GetAdminAuditLogsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<AuditLog>>();
        try
        {
            var query = _auditLogRepo.Query().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.EntityName))
            {
                query = query.Where(l => l.EntityType == request.EntityName);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<AuditLog>(items, total, request.Page, request.PageSize);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = pagedResult;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<IEnumerable<FeatureFlag>>> GetFeatureFlagsAsync(GetFeatureFlagsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<FeatureFlag>>();
        try
        {
            var flags = await _featureFlagRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = flags.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<IEnumerable<SystemSetting>>> GetSettingsAsync(GetSettingsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<SystemSetting>>();
        try
        {
            var settings = await _settingRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = settings.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }
}
