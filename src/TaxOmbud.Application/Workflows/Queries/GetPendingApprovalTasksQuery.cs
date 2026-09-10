using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Application.Workflows.Strategies;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetPendingApprovalTasksQuery() : IRequest<List<CaseApprovalTaskDto>>;

public class GetPendingApprovalTasksQueryHandler : IRequestHandler<GetPendingApprovalTasksQuery, List<CaseApprovalTaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPendingApprovalTasksQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CaseApprovalTaskDto>> Handle(GetPendingApprovalTasksQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        // 1. Fetch current user with role details (1 fast query)
        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        var userRoleId = user?.RoleId;
        var userRoleName = user?.Role?.Name ?? string.Empty;

        var isSuperAdmin = (_currentUser != null && (_currentUser.IsInRole("Super Admin") || _currentUser.IsInRole("Admin")))
                        || userRoleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)
                        || userRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        // 2. Load active workflow template with levels and targets (1 fast query)
        var activeWorkflow = await _context.Workflows
            .Include(w => w.Levels).ThenInclude(l => l.Targets)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive && !w.IsDeleted, cancellationToken)
            ?? await _context.Workflows
            .Include(w => w.Levels).ThenInclude(l => l.Targets)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.IsActive && !w.IsDeleted, cancellationToken);

        // 3. Load all pending approval tasks with related case and workflow data (1 fast query)
        var allPendingTasks = await _context.CaseApprovalTasks
            .Include(t => t.Case)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedRole)
            .Include(t => t.WorkflowInstanceLevel)
                .ThenInclude(il => il.WorkflowLevel)
                    .ThenInclude(wl => wl.Targets)
            .Where(t => t.TaskStatus == WorkflowLevelStatus.Pending && t.Case != null && t.Case.Status != CaseStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        // 4. Batch synchronize any open cases that do not yet have a pending approval task matching their current stage
        if (activeWorkflow != null && activeWorkflow.Levels.Any())
        {
            var openCases = await _context.Cases
                .Include(c => c.AssignedOfficer)
                .Where(c => c.Status != CaseStatus.Closed)
                .ToListAsync(cancellationToken);

            var tasksByCaseId = allPendingTasks
                .GroupBy(t => t.CaseId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var casesNeedingTaskSync = new List<Case>();

            foreach (var @case in openCases)
            {
                int stageLevelNum = @case.CurrentStage switch
                {
                    WorkflowStage.Intake => 1,
                    WorkflowStage.RegistrationAndAcknowledgement => 2,
                    WorkflowStage.InitialReviewAndAssignment => 3,
                    WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => 4,
                    WorkflowStage.InvestigationAndResolution => 5,
                    WorkflowStage.DecisionAndCommunication => 6,
                    WorkflowStage.ClosureAndArchiving => 7,
                    _ => 1
                };

                // Check if this case has a pending task matching its current stage level
                bool hasCurrentStageTask = tasksByCaseId.TryGetValue(@case.Id, out var caseTasks)
                    && caseTasks.Any(t => t.WorkflowInstanceLevel != null && t.WorkflowInstanceLevel.LevelNumber == stageLevelNum);

                if (!hasCurrentStageTask)
                {
                    casesNeedingTaskSync.Add(@case);
                }
            }

            if (casesNeedingTaskSync.Any())
            {
                var syncCaseIds = casesNeedingTaskSync.Select(c => c.Id).ToList();

                var existingInstances = await _context.WorkflowInstances
                    .Include(i => i.InstanceLevels)
                    .Include(i => i.ApprovalTasks)
                    .Where(i => syncCaseIds.Contains(i.CaseId) && i.Status != WorkflowStatus.Completed && i.Status != WorkflowStatus.Cancelled)
                    .ToListAsync(cancellationToken);

                var instanceByCaseId = existingInstances.ToDictionary(i => i.CaseId);

                // Get published version ID once
                var versionId = await _context.WorkflowVersions
                    .Where(v => v.WorkflowId == activeWorkflow.Id && v.IsPublished)
                    .OrderByDescending(v => v.VersionNumber)
                    .Select(v => v.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (versionId == Guid.Empty)
                {
                    var version = new WorkflowVersion(activeWorkflow.Id, 1, "{}");
                    version.Publish(currentUserId != Guid.Empty ? currentUserId : Guid.Empty);
                    _context.WorkflowVersions.Add(version);
                    await _context.SaveChangesAsync(cancellationToken);
                    versionId = version.Id;
                }

                var newlyCreatedTasks = new List<CaseApprovalTask>();

                foreach (var @case in casesNeedingTaskSync)
                {
                    var stage = @case.CurrentStage;
                    int stageLevelNum = stage switch
                    {
                        WorkflowStage.Intake => 1,
                        WorkflowStage.RegistrationAndAcknowledgement => 2,
                        WorkflowStage.InitialReviewAndAssignment => 3,
                        WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => 4,
                        WorkflowStage.InvestigationAndResolution => 5,
                        WorkflowStage.DecisionAndCommunication => 6,
                        WorkflowStage.ClosureAndArchiving => 7,
                        _ => 1
                    };

                    var targetLevel = activeWorkflow.Levels.FirstOrDefault(l => l.LevelNumber == stageLevelNum)
                                   ?? activeWorkflow.Levels.FirstOrDefault();
                    if (targetLevel == null) continue;

                    var primaryRoleTarget = targetLevel.Targets.FirstOrDefault(t => t.TargetType == WorkflowLevelTargetType.Role);
                    var primaryRoleId = primaryRoleTarget?.TargetId;

                    if (!instanceByCaseId.TryGetValue(@case.Id, out var instance))
                    {
                        instance = new WorkflowInstance(@case.Id, activeWorkflow.Id, versionId);
                        instance.CurrentLevelNumber = targetLevel.LevelNumber;
                        _context.WorkflowInstances.Add(instance);

                        foreach (var lvl in activeWorkflow.Levels.OrderBy(l => l.LevelNumber))
                        {
                            var lvlRoleTarget = lvl.Targets.FirstOrDefault(t => t.TargetType == WorkflowLevelTargetType.Role);
                            var instLvl = new WorkflowInstanceLevel(
                                instance.Id,
                                lvl.Id,
                                lvl.LevelNumber,
                                lvl.LevelNumber == targetLevel.LevelNumber ? @case.AssignedOfficer?.UserId : null,
                                lvlRoleTarget?.TargetId,
                                lvl.SlaHours,
                                lvl.EscalationHours
                            );
                            if (lvl.LevelNumber == targetLevel.LevelNumber)
                            {
                                instLvl.Status = WorkflowLevelStatus.InProgress;
                            }
                            _context.WorkflowInstanceLevels.Add(instLvl);
                            instance.InstanceLevels.Add(instLvl);
                        }

                        @case.ActiveWorkflowInstanceId = instance.Id;
                        instanceByCaseId[@case.Id] = instance;
                    }
                    else
                    {
                        instance.CurrentLevelNumber = targetLevel.LevelNumber;

                        // Mark older stage pending tasks as superseded
                        if (tasksByCaseId.TryGetValue(@case.Id, out var existingPendingTasks))
                        {
                            foreach (var oldTask in existingPendingTasks)
                            {
                                oldTask.TaskStatus = WorkflowLevelStatus.Approved;
                                oldTask.PerformedAt = DateTimeOffset.UtcNow;
                                oldTask.Comment = "Superseded by stage advance";
                            }
                        }
                    }

                    var instLevel = instance.InstanceLevels.FirstOrDefault(il => il.LevelNumber == targetLevel.LevelNumber);
                    if (instLevel != null)
                    {
                        instLevel.Status = WorkflowLevelStatus.InProgress;
                        instLevel.WorkflowLevel = targetLevel;

                        var task = new CaseApprovalTask(
                            instance.Id,
                            instLevel.Id,
                            @case.Id,
                            @case.AssignedOfficer?.UserId,
                            primaryRoleId
                        );
                        task.Case = @case;
                        task.WorkflowInstanceLevel = instLevel;
                        _context.CaseApprovalTasks.Add(task);
                        newlyCreatedTasks.Add(task);
                    }
                }

                if (newlyCreatedTasks.Any())
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    allPendingTasks.AddRange(newlyCreatedTasks);
                }
            }
        }

        // 5. Clean up any obsolete duplicate pending tasks for the same case (keeping the latest one)
        var duplicateTasks = allPendingTasks
            .GroupBy(t => t.CaseId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderByDescending(t => t.CreatedAt).Skip(1))
            .ToList();

        if (duplicateTasks.Any())
        {
            foreach (var dup in duplicateTasks)
            {
                dup.TaskStatus = WorkflowLevelStatus.Approved;
                dup.PerformedAt = DateTimeOffset.UtcNow;
                dup.Comment = "Superseded by later task";
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        var distinctPendingTasks = allPendingTasks
            .GroupBy(t => t.CaseId)
            .Select(g => g.OrderByDescending(t => t.CreatedAt).First())
            .ToList();

        if (isSuperAdmin)
        {
            return MapToDto(distinctPendingTasks);
        }

        // 6. Filter tasks visible to this officer based on workflow configuration
        // Aligned exactly with CasesService.ApplyStageQueueFilterAsync (Workflow Queue logic)
        var filteredTasks = distinctPendingTasks.Where(t =>
        {
            var stage = t.Case?.CurrentStage ?? string.Empty;
            int stageLevelNum = stage switch
            {
                WorkflowStage.Intake => 1,
                WorkflowStage.RegistrationAndAcknowledgement => 2,
                WorkflowStage.InitialReviewAndAssignment => 3,
                WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => 4,
                WorkflowStage.InvestigationAndResolution => 5,
                WorkflowStage.DecisionAndCommunication => 6,
                WorkflowStage.ClosureAndArchiving => 7,
                _ => 1
            };

            var level = t.WorkflowInstanceLevel?.WorkflowLevel
                     ?? activeWorkflow?.Levels.FirstOrDefault(l => l.LevelNumber == stageLevelNum);

            bool isEligibleForStage = level != null && user != null
                ? IsUserEligibleForLevel(user, level)
                : IsUserEligibleForStageDefault(userRoleName, stage);

            if (isEligibleForStage)
            {
                // Officer's role is assigned to this stage:
                // Sees tasks assigned to them, tasks assigned to their role, cases assigned to them,
                // or unassigned pool cases in this stage (identical to ApplyStageQueueFilterAsync)
                return (t.Case?.AssignedOfficer != null && t.Case.AssignedOfficer.UserId == currentUserId) ||
                       (t.AssignedUserId.HasValue && t.AssignedUserId.Value == currentUserId) ||
                       (userRoleId.HasValue && t.AssignedRoleId.HasValue && t.AssignedRoleId.Value == userRoleId.Value) ||
                       t.Case?.AssignedOfficerId == null;
            }
            else
            {
                // Officer's role is NOT assigned to this stage:
                // Can ONLY see tasks if specifically assigned to them as officer or task assignee
                return (t.Case?.AssignedOfficer != null && t.Case.AssignedOfficer.UserId == currentUserId) ||
                       (t.AssignedUserId.HasValue && t.AssignedUserId.Value == currentUserId);
            }
        }).ToList();

        return MapToDto(filteredTasks);
    }

    private static bool IsUserEligibleForLevel(User user, WorkflowLevel level)
    {
        var targets = level.Targets?.ToList() ?? new List<WorkflowLevelTarget>();
        if (!targets.Any())
        {
            return true;
        }

        var userTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.User).Select(t => t.TargetId).ToHashSet();
        var roleTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Role).Select(t => t.TargetId).ToHashSet();
        var deptTargetIds = targets.Where(t => t.TargetType == WorkflowLevelTargetType.Department).Select(t => t.TargetId).ToHashSet();

        if (userTargetIds.Contains(user.Id)) return true;

        bool roleMatch = user.RoleId.HasValue && roleTargetIds.Contains(user.RoleId.Value);
        bool deptMatch = !deptTargetIds.Any() || (user.DepartmentId.HasValue && deptTargetIds.Contains(user.DepartmentId.Value));

        if (roleTargetIds.Any() && deptTargetIds.Any())
        {
            return roleMatch && deptMatch;
        }
        if (roleTargetIds.Any())
        {
            return roleMatch;
        }
        if (deptTargetIds.Any())
        {
            return deptMatch;
        }

        return false;
    }

    private static bool IsUserEligibleForStageDefault(string roleName, string stage)
    {
        var r = (roleName ?? string.Empty).ToLowerInvariant();
        return stage switch
        {
            WorkflowStage.Intake => r.Contains("officer") || r.Contains("intake") || r.Contains("registry") || r.Contains("admin"),
            WorkflowStage.RegistrationAndAcknowledgement => r.Contains("officer") || r.Contains("registrar") || r.Contains("registry") || r.Contains("admin"),
            WorkflowStage.InitialReviewAndAssignment => r.Contains("manager") || r.Contains("director") || r.Contains("chief executive") || r.Contains("ce") || r.Contains("admin"),
            WorkflowStage.JurisdictionAndAdmissibility or WorkflowStage.NotAdmissible => r.Contains("legal") || r.Contains("senior") || r.Contains("manager") || r.Contains("admin"),
            WorkflowStage.InvestigationAndResolution => r.Contains("senior") || r.Contains("investigat") || r.Contains("operations") || r.Contains("admin"),
            WorkflowStage.DecisionAndCommunication => r.Contains("manager") || r.Contains("director") || r.Contains("chief executive") || r.Contains("ce") || r.Contains("admin"),
            WorkflowStage.ClosureAndArchiving => r.Contains("director") || r.Contains("chief executive") || r.Contains("ce") || r.Contains("registrar") || r.Contains("admin"),
            _ => true
        };
    }

    private static List<CaseApprovalTaskDto> MapToDto(List<CaseApprovalTask> tasks)
    {
        return tasks.Select(t =>
        {
            var level = t.WorkflowInstanceLevel?.WorkflowLevel;
            var roleName = t.AssignedRole?.Name
                        ?? (level != null && !string.IsNullOrWhiteSpace(level.Name) ? level.Name : (!string.IsNullOrWhiteSpace(t.Case?.CurrentStage) ? t.Case.CurrentStage : "Officer Queue"));

            var userName = t.AssignedUser != null
                ? $"{t.AssignedUser.FirstName} {t.AssignedUser.LastName}".Trim()
                : roleName;

            return new CaseApprovalTaskDto(
                t.Id,
                t.WorkflowInstanceId,
                t.WorkflowInstanceLevelId,
                t.CaseId,
                t.Case?.Subject ?? "Case",
                t.AssignedUserId ?? Guid.Empty,
                userName,
                t.AssignedRoleId,
                roleName,
                t.Action,
                t.TaskStatus,
                t.Comment,
                t.PerformedAt,
                t.CreatedAt
            );
        }).ToList();
    }
}
