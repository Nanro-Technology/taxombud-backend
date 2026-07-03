using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Users.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class UsersService : IUsersService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public UsersService(IApplicationDbContext context, ICurrentUser currentUser, IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<UserListDto>>> GetUsersAsync(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<UserListDto>>();
        try
        {
            var query = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(u =>
                    u.FirstName.Contains(request.Search) ||
                    u.LastName.Contains(request.Search) ||
                    u.Email!.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(u => u.Status.ToString() == request.Status);

            if (request.DepartmentId.HasValue)
                query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(u => u.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserListDto(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    $"{u.FirstName} {u.LastName}",
                    u.Email ?? string.Empty,
                    u.Phone,
                    u.JobTitle,
                    u.EmploymentType,
                    u.Department == null ? null : new DepartmentDto(u.Department.Id, u.Department.Name),
                    u.Status.ToString(),
                    u.CanSignIn,
                    u.Role == null ? null : new RoleDto(u.Role.Id, u.Role.Name),
                    u.UserType.ToString()
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Users retrieved successfully.";
            response.Data = new PagedResult<UserListDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving users.";
        }
        return response;
    }

    public async Task<Response<UserDetailDto>> GetUserByIdAsync(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserDetailDto>();
        try
        {
            var u = await _context.Users
                .Include(x => x.Department)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (u is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "User retrieved successfully.";
            response.Data = MapToDetailDto(u);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the user.";
        }
        return response;
    }

    public async Task<Response<UserDetailDto>> GetCurrentUserAsync(GetCurrentUserQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserDetailDto>();
        try
        {
            var userId = _currentUser.UserId;
            if (userId == null)
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized";
                return response;
            }

            var u = await _context.Users
                .Include(x => x.Department)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);

            if (u is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Current user retrieved successfully.";
            response.Data = MapToDetailDto(u);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the current user.";
        }
        return response;
    }

    public async Task<Response<PagedResult<AuditLogDto>>> GetAuditLogAsync(GetAuditLogQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<AuditLogDto>>();
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.EntityType))
                query = query.Where(a => a.EntityType == request.EntityType);

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(a => a.Action == request.Action);

            if (request.From.HasValue)
                query = query.Where(a => a.CreatedAt >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(a => a.CreatedAt <= request.To.Value);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditLogDto(
                    a.Id,
                    a.EntityType,
                    a.EntityId,
                    a.Action,
                    a.OldValues,
                    a.NewValues,
                    a.UserId,
                    a.ImpersonatorUserId,
                    a.IPAddress,
                    a.UserAgent,
                    a.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Audit log retrieved successfully.";
            response.Data = new PagedResult<AuditLogDto>(items, total, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the audit log.";
        }
        return response;
    }

    // ─── Commands ──────────────────────────────────────────────────────────────

    public async Task<Response<CreateUserResponse>> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateUserResponse>();
        try
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (exists)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "A user with this email already exists.";
                return response;
            }

            var user = User.Create(request.FirstName, request.LastName, new TaxOmbud.Common.Utilities.Email(request.Email), request.Phone);
            user.SetPasswordHash(_passwordHasher.Hash(request.Password));
            if (request.JobTitle is not null)
            {
                user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.JobTitle);
            }
            if (request.EmploymentType is not null)
            {
                user.SetEmploymentType(request.EmploymentType);
            }
            if (request.DepartmentId.HasValue)
            {
                user.SetDepartment(request.DepartmentId.Value);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "User created successfully.";
            response.Data = new CreateUserResponse(user.Id, $"{user.FirstName} {user.LastName}", user.Email ?? string.Empty);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the user.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            user.UpdateProfile(request.FirstName, request.LastName, request.Phone ?? user.Phone, request.JobTitle ?? user.JobTitle);
            if (request.EmploymentType is not null) user.SetEmploymentType(request.EmploymentType);
            if (request.DepartmentId.HasValue) user.SetDepartment(request.DepartmentId.Value);

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "User updated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the user.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateCurrentUserAsync(UpdateCurrentUserCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var userId = _currentUser.UserId;
            if (userId == null)
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized";
                return response;
            }

            var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            user.UpdateProfile(
                request.FirstName ?? user.FirstName,
                request.LastName ?? user.LastName,
                request.Phone ?? user.Phone,
                user.JobTitle
            );

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Profile updated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the profile.";
        }
        return response;
    }

    public async Task<Response<object?>> UpdateUserStatusAsync(UpdateUserStatusCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            if (request.Activate)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "User status updated successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the user status.";
        }
        return response;
    }

    public async Task<Response<object?>> AssignRoleAsync(AssignRolesCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            // Estate Management pattern: one role per user
            var roleId = request.RoleIds.FirstOrDefault();
            user.AssignRole(roleId == Guid.Empty ? null : roleId);

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Role assigned successfully.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while assigning role.";
        }
        return response;
    }

    // ApplyPermissionOverridesAsync removed — UserPermissionOverride entity has been deleted
    // as part of the Estate Management RBAC refactor (permissions are role-based only).

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static UserDetailDto MapToDetailDto(User u) => new(
        u.Id,
        u.FirstName,
        u.LastName,
        $"{u.FirstName} {u.LastName}",
        u.Email ?? string.Empty,
        u.Phone,
        u.AltPhone,
        u.JobTitle,
        u.EmploymentType,
        u.Department == null ? null : new DepartmentDetailDto(u.Department.Id, u.Department.Name),
        u.Status.ToString(),
        u.CanSignIn,
        u.Role == null ? null : new RoleDetailDto(u.Role.Id, u.Role.Name, u.Role.IsSystemRole),
        u.UserType.ToString()
    );
}
