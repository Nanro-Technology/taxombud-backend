using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
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

    public async Task<Guid?> SelectAssigneeAsync(Guid? roleId, Guid? specificUserId, CancellationToken cancellationToken = default)
    {
        if (specificUserId.HasValue) return specificUserId.Value;
        if (!roleId.HasValue) return null;

        var candidateUserIds = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.StaffUser && u.Status == UserStatus.Active && !u.IsDeleted && u.RoleId == roleId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (!candidateUserIds.Any())
        {
            candidateUserIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.StaffUser && u.Status == UserStatus.Active && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
        }

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
