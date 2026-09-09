using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace TaxOmbud.Application.Workflows.Commands;

public record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string Description,
    string CaseCategory = "General",
    bool IsDefault = false,
    List<CreateWorkflowLevelRequest>? Levels = null
) : IRequest<WorkflowDto>;

public class UpdateWorkflowCommandHandler : IRequestHandler<UpdateWorkflowCommand, WorkflowDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateWorkflowCommandHandler> _logger;

    public UpdateWorkflowCommandHandler(IApplicationDbContext context, ILogger<UpdateWorkflowCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowDto> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Levels)
                .ThenInclude(l => l.Targets)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workflow == null)
        {
            throw new NotFoundException(nameof(Workflow), request.Id);
        }

        // Validate mandatory AdmissibilityGate
        if (request.Levels == null || !request.Levels.Any(l => l.LevelRole == LevelRole.AdmissibilityGate))
        {
            throw new DomainException("Every workflow must include at least one level with the 'Admissibility Gate' stage role.");
        }

        if (request.IsDefault)
        {
            var defaults = _context.Workflows.Where(w => w.IsDefault && w.CaseCategory == request.CaseCategory && w.Id != request.Id);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        workflow.Name = request.Name;
        workflow.Description = request.Description;
        workflow.CaseCategory = request.CaseCategory;
        workflow.IsDefault = request.IsDefault;

        // Upsert levels in-place to preserve PKs
        var requestedLevels = request.Levels.OrderBy(l => l.LevelNumber).ToList();
        var requestedLevelNumbers = requestedLevels.Select(l => l.LevelNumber).ToHashSet();

        var now = DateTime.UtcNow;

        // Soft-delete levels no longer in request (only if not referenced by workflow instances)
        var levelsToRemove = workflow.Levels
            .Where(l => !l.IsDeleted && !requestedLevelNumbers.Contains(l.LevelNumber))
            .ToList();

        foreach (var lvlToRemove in levelsToRemove)
        {
            var isReferenced = await _context.WorkflowInstanceLevels
                .AnyAsync(il => il.WorkflowLevelId == lvlToRemove.Id, cancellationToken);
            if (!isReferenced)
            {
                lvlToRemove.IsDeleted = true;
                lvlToRemove.DeletedAt = DateTimeOffset.UtcNow;
                lvlToRemove.LastModifiedAt = now;
                foreach (var target in lvlToRemove.Targets)
                {
                    target.IsDeleted = true;
                    target.DeletedAt = DateTimeOffset.UtcNow;
                    target.LastModifiedAt = now;
                }
            }
        }

        // Update existing levels or add new ones
        foreach (var levelReq in requestedLevels)
        {
            var existingLevel = workflow.Levels.FirstOrDefault(l => !l.IsDeleted && l.LevelNumber == levelReq.LevelNumber);
            if (existingLevel != null)
            {
                existingLevel.Name = levelReq.Name;
                existingLevel.Description = levelReq.Description;
                existingLevel.LevelRole = levelReq.LevelRole;
                existingLevel.SlaHours = levelReq.SlaHours;
                existingLevel.EscalationHours = levelReq.EscalationHours;
                existingLevel.IsMandatory = levelReq.IsMandatory;
                existingLevel.RequireComment = levelReq.RequireComment;
                existingLevel.RequireAttachment = levelReq.RequireAttachment;
                existingLevel.AssignmentMode = levelReq.AssignmentMode;
                existingLevel.AssignmentAlgorithm = levelReq.AssignmentAlgorithm;
                existingLevel.LastModifiedAt = now;

                // Synchronize multi-targets: mark removed targets as deleted, add new ones
                var activeTargets = existingLevel.Targets.Where(t => !t.IsDeleted).ToList();
                var reqTargets = levelReq.Targets ?? new List<CreateWorkflowLevelTargetRequest>();
                var requestedKeys = reqTargets.Select(t => (t.TargetType, t.TargetId)).ToHashSet();

                foreach (var currentTarget in activeTargets)
                {
                    if (!requestedKeys.Contains((currentTarget.TargetType, currentTarget.TargetId)))
                    {
                        currentTarget.IsDeleted = true;
                        currentTarget.DeletedAt = DateTimeOffset.UtcNow;
                        currentTarget.LastModifiedAt = now;
                    }
                }

                var existingActiveKeys = activeTargets
                    .Where(t => !t.IsDeleted)
                    .Select(t => (t.TargetType, t.TargetId))
                    .ToHashSet();

                foreach (var t in reqTargets)
                {
                    if (!existingActiveKeys.Contains((t.TargetType, t.TargetId)))
                    {
                        var newTarget = new WorkflowLevelTarget(existingLevel.Id, t.TargetType, t.TargetId);
                        _context.WorkflowLevelTargets.Add(newTarget);
                        existingLevel.Targets.Add(newTarget);
                    }
                }
            }
            else
            {
                var newLevel = new WorkflowLevel(
                    workflow.Id,
                    levelReq.LevelNumber,
                    levelReq.Name,
                    levelReq.Description,
                    levelReq.LevelRole,
                    levelReq.AssignmentMode,
                    levelReq.AssignmentAlgorithm)
                {
                    SlaHours = levelReq.SlaHours,
                    EscalationHours = levelReq.EscalationHours,
                    IsMandatory = levelReq.IsMandatory,
                    RequireComment = levelReq.RequireComment,
                    RequireAttachment = levelReq.RequireAttachment
                };

                _context.WorkflowLevels.Add(newLevel);
                workflow.Levels.Add(newLevel);

                if (levelReq.Targets != null)
                {
                    foreach (var t in levelReq.Targets)
                    {
                        var newTarget = new WorkflowLevelTarget(newLevel.Id, t.TargetType, t.TargetId);
                        _context.WorkflowLevelTargets.Add(newTarget);
                        newLevel.Targets.Add(newTarget);
                    }
                }
            }
        }

        if (_context is DbContext dbCtx)
        {
            foreach (var entry in dbCtx.ChangeTracker.Entries())
            {
                _logger.LogInformation("ChangeTracker: Entity={Entity}, State={State}, Id={Id}",
                    entry.Entity.GetType().Name, entry.State, (entry.Entity as BaseEntity)?.Id);
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "DbUpdateConcurrencyException during workflow update for Id={WorkflowId}", request.Id);
            foreach (var entry in ex.Entries)
            {
                var dbVals = await entry.GetDatabaseValuesAsync(cancellationToken);
                _logger.LogError("Concurrency failure on: Entity={Entity}, Id={Id}, FoundInDb={FoundInDb}",
                    entry.Entity.GetType().Name,
                    (entry.Entity as BaseEntity)?.Id,
                    dbVals != null);
            }
            throw;
        }

        // Resolve display names only for active levels & active targets
        var activeLevels = workflow.Levels
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.LevelNumber)
            .ToList();

        var allActiveTargets = activeLevels
            .SelectMany(l => l.Targets)
            .Where(t => !t.IsDeleted)
            .ToList();

        var roleIds = allActiveTargets.Where(t => t.TargetType == WorkflowLevelTargetType.Role).Select(t => t.TargetId).Distinct().ToList();
        var deptIds = allActiveTargets.Where(t => t.TargetType == WorkflowLevelTargetType.Department).Select(t => t.TargetId).Distinct().ToList();
        var userIds = allActiveTargets.Where(t => t.TargetType == WorkflowLevelTargetType.User).Select(t => t.TargetId).Distinct().ToList();

        var roleNames = roleIds.Any()
            ? await _context.CustomRoles.Where(r => roleIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.Name ?? "Unknown Role", cancellationToken)
            : new Dictionary<Guid, string>();
        var deptNames = deptIds.Any()
            ? await _context.Departments.Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken)
            : new Dictionary<Guid, string>();
        var userNames = userIds.Any()
            ? await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => (u.FirstName + " " + u.LastName).Trim(), cancellationToken)
            : new Dictionary<Guid, string>();

        return new WorkflowDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.CaseCategory,
            workflow.IsActive,
            workflow.IsDefault,
            workflow.CurrentVersion,
            workflow.CreatedAt,
            activeLevels.Select(l => new WorkflowLevelDto(
                l.Id,
                l.LevelNumber,
                l.Name,
                l.Description,
                l.LevelRole,
                l.SlaHours,
                l.EscalationHours,
                l.IsMandatory,
                l.RequireComment,
                l.RequireAttachment,
                l.AssignmentMode,
                l.AssignmentAlgorithm,
                l.Targets.Where(t => !t.IsDeleted).Select(t => new WorkflowLevelTargetDto(
                    t.Id,
                    t.TargetType,
                    t.TargetId,
                    t.TargetType switch
                    {
                        WorkflowLevelTargetType.Role       => roleNames.GetValueOrDefault(t.TargetId, "Unknown Role"),
                        WorkflowLevelTargetType.Department => deptNames.GetValueOrDefault(t.TargetId, "Unknown Dept"),
                        WorkflowLevelTargetType.User       => userNames.GetValueOrDefault(t.TargetId, "Unknown User"),
                        _                                  => "Unknown"
                    }
                )).ToList()
            )).ToList()
        );
    }
}
