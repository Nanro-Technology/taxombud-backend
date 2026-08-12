using TaxOmbud.Application.Roles.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IRolesService
{
    Task<Response<CreateRoleResponse>> CreateRoleAsync(CreateRoleCommand request, CancellationToken cancellationToken = default);
    Task<Response<RoleDetailDto>> UpdateRoleAsync(UpdateRoleCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<PermissionDetailDto>>> GetPermissionsAsync(GetPermissionsQuery request, CancellationToken cancellationToken = default);
    Task<Response<RoleDetailDto>> GetRoleByIdAsync(GetRoleByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<RoleDto>>> GetRolesAsync(GetRolesQuery request, CancellationToken cancellationToken = default);
}
