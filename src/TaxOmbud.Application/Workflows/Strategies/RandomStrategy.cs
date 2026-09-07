using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class RandomStrategy : IRoutingStrategy
{
    private readonly IApplicationDbContext _context;

    public AssignmentAlgorithm Algorithm => AssignmentAlgorithm.Random;

    public RandomStrategy(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> SelectAssigneeAsync(
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken cancellationToken = default)
    {
        var (candidateUserIds, _) = await CandidateResolver.ResolveAsync(_context, targets, cancellationToken);

        if (!candidateUserIds.Any()) return null;

        var random = new Random();
        return candidateUserIds[random.Next(candidateUserIds.Count)];
    }
}
