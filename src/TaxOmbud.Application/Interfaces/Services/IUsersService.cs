using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Users.DTOs;
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

public interface IUsersService
{
    Task<Response<object?>> ApplyPermissionOverridesAsync(ApplyPermissionOverridesCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> AssignRolesAsync(AssignRolesCommand request, CancellationToken cancellationToken = default);
    Task<Response<CreateUserResponse>> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateCurrentUserAsync(UpdateCurrentUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateUserStatusAsync(UpdateUserStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AuditLogDto>>> GetAuditLogAsync(GetAuditLogQuery request, CancellationToken cancellationToken = default);
    Task<Response<UserDetailDto>> GetCurrentUserAsync(GetCurrentUserQuery request, CancellationToken cancellationToken = default);
    Task<Response<UserDetailDto>> GetUserByIdAsync(GetUserByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<UserListDto>>> GetUsersAsync(GetUsersQuery request, CancellationToken cancellationToken = default);
}
