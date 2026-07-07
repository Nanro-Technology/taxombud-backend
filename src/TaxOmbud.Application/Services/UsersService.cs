using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Users.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Services;

public class UsersService : IUsersService
{
    // ── Identity ──────────────────────────────────────────────────────────────
    private readonly UserManager<User> _userManager;

    // ── Repositories ──────────────────────────────────────────────────────────
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IGenericRepository<AuditLog> _auditRepo;

    // ── Infrastructure services ───────────────────────────────────────────────
    private readonly ICurrentUser _currentUser;

    public UsersService(
        UserManager<User> userManager,
        IGenericRepository<User> userRepo,
        IGenericRepository<Role> roleRepo,
        IGenericRepository<AuditLog> auditRepo,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _auditRepo = auditRepo;
        _currentUser = currentUser;
    }

    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<Response<PagedResult<UserListDto>>> GetUsersAsync(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<UserListDto>>();
        try
        {
            var query = _userRepo.Query()
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
            var u = await _userRepo.Query()
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

            var u = await _userRepo.Query()
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
            var query = _auditRepo.Query()
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

    /// <summary>
    /// Admin creates a staff user directly (bypass self-registration flow).
    /// Password hashing is handled by UserManager — IPasswordHasher is no longer injected.
    /// Only StaffUser accounts can be created here.
    /// </summary>
    public async Task<Response<CreateUserResponse>> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateUserResponse>();
        try
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var existing = await _userManager.FindByEmailAsync(emailNormalized);
            if (existing is not null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "A user with this email already exists.";
                return response;
            }

            var user = User.Create(
                request.FirstName,
                request.LastName,
                new Email(emailNormalized),
                request.Phone,
                UserType.StaffUser);

            if (request.JobTitle is not null)
                user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.JobTitle);

            if (request.EmploymentType is not null)
                user.SetEmploymentType(request.EmploymentType);

            if (request.DepartmentId.HasValue)
                user.SetDepartment(request.DepartmentId.Value);

            // UserManager handles password hashing, security stamp, lockout seeding
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = string.Join("; ", result.Errors.Select(e => e.Description));
                return response;
            }

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
            var user = await _userRepo.GetByIdAsync(request.Id);
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            user.UpdateProfile(request.FirstName, request.LastName, request.Phone ?? user.Phone, request.JobTitle ?? user.JobTitle);
            if (request.EmploymentType is not null) user.SetEmploymentType(request.EmploymentType);
            if (request.DepartmentId.HasValue) user.SetDepartment(request.DepartmentId.Value);

            await _userManager.UpdateAsync(user);

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

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
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

            await _userManager.UpdateAsync(user);

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
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            if (request.Activate)
                user.Activate();
            else
                user.Deactivate();

            await _userManager.UpdateAsync(user);

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

    /// <summary>
    /// Assigns a staff role to a user.
    /// RULE: Only StaffUser accounts can be assigned roles.
    /// Taxpayers are identified by UserType — they must never be assigned a role.
    /// </summary>
    public async Task<Response<object?>> AssignRoleAsync(AssignRolesCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var user = await _userRepo.FindAsync(u => u.Id == request.Id);
            if (user is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "User not found.";
                return response;
            }

            // Guard: roles are exclusively for staff users
            if (user.UserType != UserType.StaffUser)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Roles can only be assigned to staff accounts. Taxpayers and guests are identified by their UserType — they do not use roles.";
                return response;
            }

            // Validate the role exists before assigning
            var roleId = request.RoleIds.FirstOrDefault();
            if (roleId != Guid.Empty)
            {
                var roleExists = await _roleRepo.ExistsAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "The specified role does not exist.";
                    return response;
                }
            }

            user.AssignRole(roleId == Guid.Empty ? null : roleId);

            await _userManager.UpdateAsync(user);

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

    // ─── Private helpers ───────────────────────────────────────────────────────

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
