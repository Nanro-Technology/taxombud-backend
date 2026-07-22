using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class RoutingStrategyFactory
{
    private readonly IEnumerable<IRoutingStrategy> _strategies;

    public RoutingStrategyFactory(IEnumerable<IRoutingStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IRoutingStrategy GetStrategy(AssignmentAlgorithm algorithm)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Algorithm == algorithm);
        return strategy ?? _strategies.First(s => s.Algorithm == AssignmentAlgorithm.RoundRobin);
    }
}
