using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Workflows;
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

    public async Task<Guid?> SelectAssigneeAsync(
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken cancellationToken = default)
    {
        var (candidateUserIds, _) = await CandidateResolver.ResolveAsync(_context, targets, cancellationToken);

        if (!candidateUserIds.Any()) return null;

        // Select candidate with fewest pending approval tasks
        var workloadCounts = await _context.CaseApprovalTasks
            .AsNoTracking()
            .Where(t => t.AssignedUserId.HasValue
                     && candidateUserIds.Contains(t.AssignedUserId.Value)
                     && t.TaskStatus == WorkflowLevelStatus.Pending)
            .GroupBy(t => t.AssignedUserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count, cancellationToken);

        return candidateUserIds
            .OrderBy(id => workloadCounts.ContainsKey(id) ? workloadCounts[id] : 0)
            .First();
    }
}
