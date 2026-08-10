using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
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
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsersService> _logger;

    public UsersService(
        UserManager<User> userManager,
        IGenericRepository<User> userRepo,
        IGenericRepository<Role> roleRepo,
        IGenericRepository<AuditLog> auditRepo,
        ICurrentUser currentUser,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<UsersService> logger)
    {
        _userManager = userManager;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _auditRepo = auditRepo;
        _currentUser = currentUser;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
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

            var generatedPassword = !string.IsNullOrWhiteSpace(request.Password)
                ? request.Password
                : GenerateTemporaryPassword();

            var user = User.Create(
                request.FirstName,
                request.LastName,
                new Email(emailNormalized),
                request.Phone,
                UserType.StaffUser);

            user.MustChangePassword = true;

            if (request.RoleId.HasValue && request.RoleId.Value != Guid.Empty)
                user.AssignRole(request.RoleId.Value);

            if (request.JobTitle is not null)
                user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.JobTitle);

            if (request.EmploymentType is not null)
                user.SetEmploymentType(request.EmploymentType);

            if (request.DepartmentId.HasValue)
                user.SetDepartment(request.DepartmentId.Value);

            // UserManager handles password hashing, security stamp, lockout seeding
            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (!result.Succeeded)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = string.Join("; ", result.Errors.Select(e => e.Description));
                return response;
            }

            await _userRepo.SaveAsync();

            var baseUrl = Helper.GetAppBaseUrl(_configuration);
            var loginUrl = $"{baseUrl}/staff-login";

            var htmlBody = $"""
                <div style="font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;">
                  <div style="background:#114a31;padding:28px 32px;text-align:center;border-bottom:4px solid #c9a227;">
                    <h1 style="color:#ffffff;font-size:1.2rem;margin:0 0 4px;letter-spacing:.5px;text-transform:uppercase;">OFFICE OF THE TAX OMBUD</h1>
                    <p style="color:rgba(255,255,255,.75);font-size:.85rem;margin:0;">Federal Republic of Nigeria</p>
                  </div>
                  <div style="padding:32px;background:#ffffff;color:#333333;font-size:.95rem;line-height:1.7;">
                    <h2 style="color:#114a31;font-size:1.2rem;margin-top:0;">Welcome to Tax Ombud Portal</h2>
                    <p>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                    <p>An administrator has created your staff account on the <strong>Tax Ombud Office Portal</strong>.</p>
                    <div style="background:#f8f9fa;border-left:4px solid #114a31;padding:16px 20px;margin:24px 0;border-radius:4px;">
                      <p style="margin:0 0 8px;"><strong>Login Email:</strong> {user.Email}</p>
                      <p style="margin:0;"><strong>Temporary Password:</strong> <code style="background:#e9ecef;padding:2px 8px;border-radius:4px;font-weight:bold;color:#114a31;">{generatedPassword}</code></p>
                    </div>
                    <p>Please click the button below to sign into your account. You will be required to change your password upon your first sign-in.</p>
                    <div style="text-align:center;margin:32px 0;">
                      <a href="{loginUrl}" style="background:#114a31;color:#ffffff;padding:14px 32px;border-radius:6px;text-decoration:none;font-weight:bold;font-size:1rem;display:inline-block;">Sign In to Staff Portal</a>
                    </div>
                    <p style="font-size:.85rem;color:#666666;">If you have any questions, please contact your system administrator.</p>
                  </div>
                  <div style="background:#114a31;padding:20px 32px;text-align:center;">
                    <p style="color:#c9a227;font-size:.9rem;font-weight:bold;margin:4px 0;">Office of the Tax Ombud</p>
                    <p style="color:rgba(255,255,255,.6);font-size:.75rem;margin:4px 0;">Federal Republic of Nigeria</p>
                  </div>
                </div>
                """;

            try
            {
                await _emailService.SendAsync(
                    to: user.Email ?? string.Empty,
                    subject: "Your Tax Ombud Staff Account Credentials",
                    htmlBody: htmlBody,
                    cancellationToken: cancellationToken);

                // Send audit status copy to Initiator / Administrator
                var initiatorEmail = _currentUser.Email;
                if (!string.IsNullOrWhiteSpace(initiatorEmail) && initiatorEmail != user.Email)
                {
                    var initiatorHtml = $"""
                        <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                          <h3 style="color:#114a31;margin-top:0;">Audit Notification: Staff Account Created</h3>
                          <p>Hello <strong>{_currentUser.FullName ?? "Administrator"}</strong>,</p>
                          <p>You have created a staff account for <strong>{user.FirstName} {user.LastName}</strong> ({user.Email}).</p>
                          <p><strong>Status:</strong> Credentials email dispatched to recipient.</p>
                        </div>
                        """;
                    await _emailService.SendAsync(initiatorEmail, $"[Audit Copy] Staff Account Created: {user.FirstName} {user.LastName}", initiatorHtml, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send onboarding email to {Email}", user.Email);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "User created successfully and welcome email sent.";

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

            // Send notification to User and Initiator
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var userNoticeHtml = $"""
                        <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                          <h3 style="color:#114a31;margin-top:0;">Tax Ombud Account Update</h3>
                          <p>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                          <p>Your profile details have been updated by an administrator.</p>
                        </div>
                        """;
                    await _emailService.SendAsync(user.Email, "Tax Ombud Account Updated", userNoticeHtml, cancellationToken);

                    var initiatorEmail = _currentUser.Email;
                    if (!string.IsNullOrWhiteSpace(initiatorEmail) && initiatorEmail != user.Email)
                    {
                        var initiatorHtml = $"""
                            <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                              <h3 style="color:#114a31;margin-top:0;">Audit Notification: User Updated</h3>
                              <p>Hello <strong>{_currentUser.FullName ?? "Administrator"}</strong>,</p>
                              <p>You updated profile details for <strong>{user.FirstName} {user.LastName}</strong> ({user.Email}).</p>
                              <p><strong>Status:</strong> Notification dispatched to user.</p>
                            </div>
                            """;
                        await _emailService.SendAsync(initiatorEmail, $"[Audit Copy] User Updated: {user.FirstName} {user.LastName}", initiatorHtml, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send user update notifications for {Email}", user.Email);
                }
            }

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

            // Dispatch notification to user and initiator
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var statusText = request.Activate ? "ACTIVATED" : "DEACTIVATED";
                    var userNoticeHtml = $"""
                        <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                          <h3 style="color:#114a31;margin-top:0;">Tax Ombud Account Status Changed</h3>
                          <p>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                          <p>Your Tax Ombud staff account status has been updated to <strong>{statusText}</strong>.</p>
                        </div>
                        """;
                    await _emailService.SendAsync(user.Email, $"Tax Ombud Account Status: {statusText}", userNoticeHtml, cancellationToken);

                    var initiatorEmail = _currentUser.Email;
                    if (!string.IsNullOrWhiteSpace(initiatorEmail) && initiatorEmail != user.Email)
                    {
                        var initiatorHtml = $"""
                            <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                              <h3 style="color:#114a31;margin-top:0;">Audit Notification: User Status Changed</h3>
                              <p>Hello <strong>{_currentUser.FullName ?? "Administrator"}</strong>,</p>
                              <p>You changed account status for <strong>{user.FirstName} {user.LastName}</strong> ({user.Email}) to <strong>{statusText}</strong>.</p>
                              <p><strong>Status:</strong> Notification dispatched to user.</p>
                            </div>
                            """;
                        await _emailService.SendAsync(initiatorEmail, $"[Audit Copy] Account Status Changed: {user.FirstName} {user.LastName} ({statusText})", initiatorHtml, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send status update notification to {Email}", user.Email);
                }
            }

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
            Role? roleObj = null;
            if (roleId != Guid.Empty)
            {
                roleObj = await _roleRepo.GetByIdAsync(roleId);
                if (roleObj is null)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "The specified role does not exist.";
                    return response;
                }
            }

            user.AssignRole(roleId == Guid.Empty ? null : roleId);

            await _userManager.UpdateAsync(user);
            await _userRepo.SaveAsync();

            // Send role update notifications
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var roleName = roleObj?.Name ?? "No Role";
                    var userNoticeHtml = $"""
                        <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                          <h3 style="color:#114a31;margin-top:0;">Tax Ombud Role Assignment Updated</h3>
                          <p>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                          <p>Your staff role has been updated to <strong>{roleName}</strong>.</p>
                        </div>
                        """;
                    await _emailService.SendAsync(user.Email, "Tax Ombud Role Updated", userNoticeHtml, cancellationToken);

                    var initiatorEmail = _currentUser.Email;
                    if (!string.IsNullOrWhiteSpace(initiatorEmail) && initiatorEmail != user.Email)
                    {
                        var initiatorHtml = $"""
                            <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                              <h3 style="color:#114a31;margin-top:0;">Audit Notification: Role Assigned</h3>
                              <p>Hello <strong>{_currentUser.FullName ?? "Administrator"}</strong>,</p>
                              <p>You assigned role <strong>{roleName}</strong> to <strong>{user.FirstName} {user.LastName}</strong> ({user.Email}).</p>
                              <p><strong>Status:</strong> Notification dispatched to user.</p>
                            </div>
                            """;
                        await _emailService.SendAsync(initiatorEmail, $"[Audit Copy] Role Assigned: {user.FirstName} {user.LastName} ({roleName})", initiatorHtml, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send role assignment notification to {Email}", user.Email);
                }
            }

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

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*";

        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);

        var u = upper[bytes[0] % upper.Length];
        var l = lower[bytes[1] % lower.Length];
        var d = digits[bytes[2] % digits.Length];
        var s = special[bytes[3] % special.Length];

        var rest = new char[4];
        const string all = upper + lower + digits + special;
        for (int i = 0; i < 4; i++)
        {
            rest[i] = all[bytes[4 + i] % all.Length];
        }

        return $"TxObud#{u}{l}{d}{s}{new string(rest)}";
    }

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
