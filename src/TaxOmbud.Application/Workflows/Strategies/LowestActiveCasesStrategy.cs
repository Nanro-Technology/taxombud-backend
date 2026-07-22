using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

public class LowestActiveCasesStrategy : IRoutingStrategy
{
    private readonly IApplicationDbContext _context;

    public AssignmentAlgorithm Algorithm => AssignmentAlgorithm.LowestActiveCases;

    public LowestActiveCasesStrategy(IApplicationDbContext context)
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

        // Group active cases assigned to officers
        var activeCaseCounts = await _context.Cases
            .AsNoTracking()
            .Where(c => c.AssignedOfficerId.HasValue && candidateUserIds.Contains(c.AssignedOfficerId.Value) && c.Status != CaseStatus.Closed)
            .GroupBy(c => c.AssignedOfficerId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count, cancellationToken);

        return candidateUserIds
            .OrderBy(id => activeCaseCounts.ContainsKey(id) ? activeCaseCounts[id] : 0)
            .First();
    }
}
