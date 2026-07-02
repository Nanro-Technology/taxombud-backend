using TaxOmbud.Application.Users.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IUsersService
{
    Task<Response<object?>> AssignRoleAsync(AssignRolesCommand request, CancellationToken cancellationToken = default);
    Task<Response<CreateUserResponse>> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateCurrentUserAsync(UpdateCurrentUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateUserStatusAsync(UpdateUserStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<AuditLogDto>>> GetAuditLogAsync(GetAuditLogQuery request, CancellationToken cancellationToken = default);
    Task<Response<UserDetailDto>> GetCurrentUserAsync(GetCurrentUserQuery request, CancellationToken cancellationToken = default);
    Task<Response<UserDetailDto>> GetUserByIdAsync(GetUserByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<UserListDto>>> GetUsersAsync(GetUsersQuery request, CancellationToken cancellationToken = default);
}
