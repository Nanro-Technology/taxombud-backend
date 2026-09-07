using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Strategies;

/// <summary>
/// Shared helper that resolves the candidate user pool from a multi-target
/// WorkflowLevelTarget list, applying department ∩ role intersection logic.
/// All routing strategies call this to get their candidate list before applying
/// their specific selection algorithm.
/// </summary>
internal static class CandidateResolver
{
    /// <summary>
    /// Returns the resolved candidate user IDs based on multi-target configuration.
    ///
    /// Logic:
    ///   1. Specific User targets → always included.
    ///   2. Department targets → filter staff to those departments.
    ///   3. Role targets → intersect with dept-scoped pool (or all active staff if no dept targets).
    ///   4. If no role targets either, all active staff in specified departments qualify.
    ///   5. If no targets at all → falls back to all active staff users.
    ///
    /// Returns (candidateIds, firstRoleId) where firstRoleId is used for task queue visibility.
    /// </summary>
    public static async Task<(List<Guid> CandidateIds, Guid? PrimaryRoleId)> ResolveAsync(
        IApplicationDbContext context,
        IEnumerable<WorkflowLevelTarget> targets,
        CancellationToken ct)
    {
        var targetList = targets.ToList();

        var userTargetIds = targetList
            .Where(t => t.TargetType == WorkflowLevelTargetType.User)
            .Select(t => t.TargetId)
            .ToHashSet();

        var roleTargetIds = targetList
            .Where(t => t.TargetType == WorkflowLevelTargetType.Role)
            .Select(t => t.TargetId)
            .ToHashSet();

        var deptTargetIds = targetList
            .Where(t => t.TargetType == WorkflowLevelTargetType.Department)
            .Select(t => t.TargetId)
            .ToHashSet();

        // Build base query: active staff only
        var baseQuery = context.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.StaffUser
                     && u.Status == UserStatus.Active
                     && !u.IsDeleted);

        // Apply department filter if departments are specified
        if (deptTargetIds.Any())
        {
            baseQuery = baseQuery.Where(u => u.DepartmentId.HasValue && deptTargetIds.Contains(u.DepartmentId.Value));
        }

        // Apply role filter if roles are specified
        if (roleTargetIds.Any())
        {
            baseQuery = baseQuery.Where(u => u.RoleId.HasValue && roleTargetIds.Contains(u.RoleId.Value));
        }

        var poolIds = await baseQuery.Select(u => u.Id).ToListAsync(ct);

        // Union specific user targets
        if (userTargetIds.Any())
        {
            var specificUserIds = await context.Users
                .AsNoTracking()
                .Where(u => userTargetIds.Contains(u.Id) && u.Status == UserStatus.Active && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync(ct);

            poolIds = poolIds.Union(specificUserIds).Distinct().ToList();
        }

        // If nothing resolved, fall back to all active staff (role-pool task)
        if (!poolIds.Any())
        {
            poolIds = await context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.StaffUser && u.Status == UserStatus.Active && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync(ct);
        }

        var primaryRoleId = roleTargetIds.Any() ? roleTargetIds.First() : (Guid?)null;
        return (poolIds, primaryRoleId);
    }
}
