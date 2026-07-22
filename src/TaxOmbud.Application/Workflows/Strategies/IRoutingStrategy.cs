using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public interface IRoutingStrategy
{
    AssignmentAlgorithm Algorithm { get; }
    Task<Guid?> SelectAssigneeAsync(Guid? roleId, Guid? specificUserId, CancellationToken cancellationToken = default);
}
