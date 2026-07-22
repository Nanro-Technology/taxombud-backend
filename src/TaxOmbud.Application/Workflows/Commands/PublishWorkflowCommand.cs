using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Application.Workflows.Commands;

public record PublishWorkflowCommand(Guid WorkflowId) : IRequest<WorkflowVersionDto>;

public class PublishWorkflowCommandHandler : IRequestHandler<PublishWorkflowCommand, WorkflowVersionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public PublishWorkflowCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WorkflowVersionDto> Handle(PublishWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Levels)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId, cancellationToken);

        if (workflow == null)
        {
            throw new NotFoundException(nameof(Workflow), request.WorkflowId);
        }

        if (!workflow.Levels.Any())
        {
            throw new DomainException("Cannot publish a workflow with no approval levels configured.");
        }

        // Serialize snapshot of workflow configuration
        var snapshot = JsonSerializer.Serialize(workflow, new JsonSerializerOptions
        {
            ReferenceHandler = global::System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        var versionNumber = workflow.CurrentVersion;
        var version = new WorkflowVersion(workflow.Id, versionNumber, snapshot);
        version.Publish(_currentUser.UserId ?? Guid.Empty);

        _context.WorkflowVersions.Add(version);
        
        // Bump current version for next edits
        workflow.CurrentVersion += 1;

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkflowVersionDto(
            version.Id,
            version.WorkflowId,
            version.VersionNumber,
            version.IsPublished,
            version.PublishedAt
        );
    }
}
