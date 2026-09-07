using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class RoundRobinStrategy : IRoutingStrategy
{
    private readonly IApplicationDbContext _context;

    public AssignmentAlgorithm Algorithm => AssignmentAlgorithm.RoundRobin;

    public RoundRobinStrategy(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> SelectAssigneeAsync(
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken cancellationToken = default)
    {
        var (candidateUserIds, _) = await CandidateResolver.ResolveAsync(_context, targets, cancellationToken);

        if (!candidateUserIds.Any()) return null;

        // Select the candidate who was assigned a task least recently
        var lastAssignedUser = await _context.CaseApprovalTasks
            .AsNoTracking()
            .Where(t => t.AssignedUserId.HasValue && candidateUserIds.Contains(t.AssignedUserId.Value))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.AssignedUserId ?? Guid.Empty)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedUser == Guid.Empty)
        {
            return candidateUserIds.First();
        }

        var index = candidateUserIds.IndexOf(lastAssignedUser);
        var nextIndex = (index + 1) % candidateUserIds.Count;
        return candidateUserIds[nextIndex];
    }
}
