using TaxOmbud.Application.System.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISystemService
{
    Task<Response<Guid>> CreateAnnouncementAsync(CreateAnnouncementCommand request, CancellationToken cancellationToken = default);
    Task<Response<ImpersonationResponseDto>> ImpersonateUserAsync(ImpersonateUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<StopImpersonationResponseDto>> StopImpersonationAsync(StopImpersonationCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ToggleFeatureFlagAsync(ToggleFeatureFlagCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateSettingAsync(UpdateSettingCommand request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AuditLog>>> GetAdminAuditLogsAsync(GetAdminAuditLogsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<FeatureFlag>>> GetFeatureFlagsAsync(GetFeatureFlagsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<SystemSetting>>> GetSettingsAsync(GetSettingsQuery request, CancellationToken cancellationToken = default);
}
