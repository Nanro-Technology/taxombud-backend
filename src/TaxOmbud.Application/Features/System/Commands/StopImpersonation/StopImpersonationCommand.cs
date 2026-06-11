using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.System.Commands.StopImpersonation;

// ─── Command ─────────────────────────────────────────────────────────────────

public record StopImpersonationCommand() : IRequest<Result<StopImpersonationResponseDto>>;

public record StopImpersonationResponseDto(string Message);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class StopImpersonationCommandHandler : IRequestHandler<StopImpersonationCommand, Result<StopImpersonationResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public StopImpersonationCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<StopImpersonationResponseDto>> Handle(StopImpersonationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = "ImpersonationEnd",
            EntityType = "Users",
            EntityId = currentUserId,
            OldValues = $"User: {currentUserId}",
            NewValues = "Impersonation Session Terminated",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<StopImpersonationResponseDto>.Success(new StopImpersonationResponseDto("Impersonation session terminated successfully."));
    }
}
