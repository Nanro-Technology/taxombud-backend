using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetPendingApprovalTasksQuery() : IRequest<List<CaseApprovalTaskDto>>;

public class GetPendingApprovalTasksQueryHandler : IRequestHandler<GetPendingApprovalTasksQuery, List<CaseApprovalTaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPendingApprovalTasksQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CaseApprovalTaskDto>> Handle(GetPendingApprovalTasksQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        // Fetch current user with role details
        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        var userRoleId = user?.RoleId;
        var userRoleName = user?.Role?.Name ?? string.Empty;
        var isSuperAdmin = userRoleName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                           userRoleName.Contains("Admin", StringComparison.OrdinalIgnoreCase);

        var query = _context.CaseApprovalTasks
            .Include(t => t.Case)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedRole)
            .AsNoTracking()
            .Where(t => t.TaskStatus == WorkflowLevelStatus.Pending);

        if (!isSuperAdmin)
        {
            query = query.Where(t =>
                (t.AssignedUserId != Guid.Empty && t.AssignedUserId == currentUserId) ||
                (userRoleId.HasValue && t.AssignedRoleId.HasValue && t.AssignedRoleId.Value == userRoleId.Value) ||
                (!string.IsNullOrEmpty(userRoleName) && t.AssignedRole != null && t.AssignedRole.Name == userRoleName)
            );
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => new CaseApprovalTaskDto(
            t.Id,
            t.WorkflowInstanceId,
            t.WorkflowInstanceLevelId,
            t.CaseId,
            t.Case?.Subject ?? "Case",
            t.AssignedUserId,
            t.AssignedUser != null ? $"{t.AssignedUser.FirstName} {t.AssignedUser.LastName}" : (t.AssignedRole != null ? t.AssignedRole.Name : "Assigned Officer"),
            t.AssignedRoleId,
            t.AssignedRole?.Name,
            t.Action,
            t.TaskStatus,
            t.Comment,
            t.PerformedAt,
            t.CreatedAt
        )).ToList();
    }
}
