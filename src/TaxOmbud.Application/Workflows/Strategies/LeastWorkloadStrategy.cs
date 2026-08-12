using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class LeastWorkloadStrategy : IRoutingStrategy
{
    private readonly IApplicationDbContext _context;

    public AssignmentAlgorithm Algorithm => AssignmentAlgorithm.LeastWorkload;

    public LeastWorkloadStrategy(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> SelectAssigneeAsync(Guid? roleId, Guid? specificUserId, CancellationToken cancellationToken = default)
    {
        if (specificUserId.HasValue) return specificUserId.Value;
        if (!roleId.HasValue) return null;

        var candidateUserIds = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.StaffUser && u.Status == UserStatus.Active && !u.IsDeleted && u.RoleId == roleId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (!candidateUserIds.Any()) return null;

        // Group active tasks by user ID to find officer with lowest count
        var workloadCounts = await _context.CaseApprovalTasks
            .AsNoTracking()
            .Where(t => t.AssignedUserId.HasValue && candidateUserIds.Contains(t.AssignedUserId.Value) && t.TaskStatus == WorkflowLevelStatus.Pending)
            .GroupBy(t => t.AssignedUserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count, cancellationToken);

        var leastWorkloadUser = candidateUserIds
            .OrderBy(id => workloadCounts.ContainsKey(id) ? workloadCounts[id] : 0)
            .First();

        return leastWorkloadUser;
    }
}
