using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Application.Workflows.Commands;

public record ReassignCaseTaskCommand(
    Guid TaskId,
    Guid NewAssigneeUserId,
    string Reason
) : IRequest<bool>;

public class ReassignCaseTaskCommandHandler : IRequestHandler<ReassignCaseTaskCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ReassignCaseTaskCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ReassignCaseTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CaseApprovalTasks
            .Include(t => t.WorkflowInstance)
            .Include(t => t.WorkflowInstanceLevel)
                .ThenInclude(il => il.WorkflowLevel)
            .Include(t => t.Case)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(CaseApprovalTask), request.TaskId);
        }

        var newAssignee = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.NewAssigneeUserId, cancellationToken);
        if (newAssignee == null)
        {
            throw new NotFoundException("User", request.NewAssigneeUserId);
        }

        var previousAssigneeId = task.AssignedUserId;
        task.AssignedUserId = request.NewAssigneeUserId;
        task.WorkflowInstanceLevel.AssignedUserId = request.NewAssigneeUserId;

        task.Case.Assign(request.NewAssigneeUserId, _currentUser.UserId ?? Guid.Empty);

        var audit = new CaseWorkflowAuditLog(
            task.CaseId,
            task.WorkflowInstanceId,
            _currentUser.UserId ?? Guid.Empty,
            "Manager/Admin",
            "Reassigned",
            task.WorkflowInstance.Status.ToString(),
            task.WorkflowInstance.Status.ToString(),
            task.WorkflowInstanceLevel.LevelNumber,
            task.WorkflowInstanceLevel.WorkflowLevel.Name,
            request.Reason,
            previousAssigneeId,
            request.NewAssigneeUserId
        );
        _context.CaseWorkflowAuditLogs.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
