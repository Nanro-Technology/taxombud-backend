using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Application.Workflows.Commands;

public record DeleteWorkflowCommand(Guid Id) : IRequest<bool>;

public class DeleteWorkflowCommandHandler : IRequestHandler<DeleteWorkflowCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkflowCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Levels)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workflow == null)
        {
            throw new NotFoundException(nameof(Workflow), request.Id);
        }

        if (workflow.Levels.Any())
        {
            _context.WorkflowLevels.RemoveRange(workflow.Levels);
        }

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
