using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.System.Commands.ImpersonateUser;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ImpersonateUserCommand(Guid UserId) : IRequest<Result<ImpersonationResponseDto>>;

public record ImpersonationResponseDto(
    string Message,
    string Token,
    Guid TargetUserId,
    Guid ImpersonatorUserId
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class ImpersonateUserCommandValidator : AbstractValidator<ImpersonateUserCommand>
{
    public ImpersonateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ImpersonateUserCommandHandler : IRequestHandler<ImpersonateUserCommand, Result<ImpersonationResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ITokenService _tokenService;

    public ImpersonateUserCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, ITokenService tokenService)
    {
        _context = context;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    public async Task<Result<ImpersonationResponseDto>> Handle(ImpersonateUserCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? Guid.Empty;
        if (adminUserId == request.UserId)
            return Result<ImpersonationResponseDto>.Failure("Cannot impersonate yourself.");

        var targetUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (targetUser == null)
            return Result<ImpersonationResponseDto>.NotFound("Target user not found.");

        if (targetUser.UserRoles.Any(ur => ur.Role.Code == "superadmin"))
        {
            return Result<ImpersonationResponseDto>.Failure("Cannot impersonate another Super Admin without dual-control approval.");
        }

        // Generate token representing the target user, but marked as impersonated
        var roles = targetUser.UserRoles.Select(ur => ur.Role.Name).ToList();
        
        // Retrieve permissions - check if RolePermissions is populated or if we need to load it.
        // Wait, the original code had:
        // var permissions = targetUser.UserRoles.SelectMany(ur => ur.Role.RolePermissions).Select(rp => rp.PermissionCode).Distinct().ToList();
        // Since the Include above loaded UserRoles and Role, but maybe not RolePermissions, let's load RolePermissions if it is not populated, or let EF do lazy loading / explicit loading.
        // Wait, to be safe and avoid issues, we can query permissions from the DB context:
        var roleIds = targetUser.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        var token = _tokenService.GenerateAccessToken(targetUser.Id, targetUser.Email, roles, permissions);

        // Log impersonation event
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            Action = "ImpersonationStart",
            EntityType = "Users",
            EntityId = targetUser.Id,
            OldValues = $"Admin: {adminUserId}",
            NewValues = $"Impersonating Target: {targetUser.Email}",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ImpersonationResponseDto(
            $"Now impersonating {targetUser.FullName}.",
            token,
            targetUser.Id,
            adminUserId
        );

        return Result<ImpersonationResponseDto>.Success(response);
    }
}
