using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
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

    public async Task<Guid?> SelectAssigneeAsync(Guid? roleId, Guid? specificUserId, CancellationToken cancellationToken = default)
    {
        if (specificUserId.HasValue) return specificUserId.Value;
        if (!roleId.HasValue) return null;

        var firstStaff = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.StaffUser && u.Status == UserStatus.Active && !u.IsDeleted && u.RoleId == roleId)
            .OrderBy(u => u.CreatedAt)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return firstStaff;
    }
}
