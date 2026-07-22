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

        var tasks = await _context.CaseApprovalTasks
            .Include(t => t.Case)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedRole)
            .AsNoTracking()
            .Where(t => t.AssignedUserId == currentUserId && t.TaskStatus == WorkflowLevelStatus.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => new CaseApprovalTaskDto(
            t.Id,
            t.WorkflowInstanceId,
            t.WorkflowInstanceLevelId,
            t.CaseId,
            t.Case?.Subject ?? "Case",
            t.AssignedUserId,
            t.AssignedUser != null ? $"{t.AssignedUser.FirstName} {t.AssignedUser.LastName}" : "Assigned User",
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
