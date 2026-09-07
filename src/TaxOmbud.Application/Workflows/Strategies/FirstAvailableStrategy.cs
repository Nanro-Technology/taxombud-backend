using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class FirstAvailableStrategy : IRoutingStrategy
{
    private readonly IApplicationDbContext _context;

    public AssignmentAlgorithm Algorithm => AssignmentAlgorithm.FirstAvailable;

    public FirstAvailableStrategy(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> SelectAssigneeAsync(
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken cancellationToken = default)
    {
        var (candidateUserIds, _) = await CandidateResolver.ResolveAsync(_context, targets, cancellationToken);

        // Simply return the first in the resolved pool (earliest created)
        return candidateUserIds.Any() ? candidateUserIds.First() : null;
    }
}
