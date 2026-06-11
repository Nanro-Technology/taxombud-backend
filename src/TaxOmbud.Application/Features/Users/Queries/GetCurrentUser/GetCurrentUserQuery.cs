using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Users.Queries.GetUserById;

namespace TaxOmbud.Application.Features.Users.Queries.GetCurrentUser;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetCurrentUserQuery : IRequest<Result<UserDetailDto>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDetailDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result<UserDetailDto>.Failure("User is not authenticated.");

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissionOverrides)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
            return Result<UserDetailDto>.NotFound("User not found.");

        return Result<UserDetailDto>.Success(new UserDetailDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.Phone,
            user.AltPhone,
            user.JobTitle,
            user.EmploymentType,
            user.Department != null ? new DepartmentDetailDto(user.Department.Id, user.Department.Name) : null,
            user.Status.ToString(),
            user.CanSignIn,
            user.UserRoles.Select(ur => new RoleDetailDto(ur.Role!.Id, ur.Role.Name, ur.Role.Code)),
            user.UserPermissionOverrides.Select(o => new PermissionOverrideDetailDto(o.PermissionCode, o.Mode))
        ));
    }
}
