using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public interface IRoutingStrategy
{
    AssignmentAlgorithm Algorithm { get; }

    /// <summary>
    /// Resolves the best assignee from the given multi-target configuration.
    /// Routing logic:
    ///   1. Collect User targets → specific user IDs always eligible.
    ///   2. Collect Department targets → intersect staff by those departments.
    ///   3. Collect Role targets → further intersect (or all-dept pool if no dept targets).
    ///   4. Union specific users + intersected pool → apply algorithm → return winner.
    ///   Returns null if no candidate found (falls back to role-pool task).
    /// </summary>
    Task<Guid?> SelectAssigneeAsync(
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken cancellationToken = default);
}
