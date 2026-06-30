using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.System.DTOs;
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
