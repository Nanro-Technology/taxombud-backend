using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Workflows;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

// ─── GET: List Stage Library ──────────────────────────────────────────────────

public record GetStageLibraryQuery : IRequest<List<WorkflowStageLibraryItemDto>>;

public class GetStageLibraryQueryHandler : IRequestHandler<GetStageLibraryQuery, List<WorkflowStageLibraryItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStageLibraryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowStageLibraryItemDto>> Handle(GetStageLibraryQuery request, CancellationToken cancellationToken)
    {
        return await _context.WorkflowStageLibrary
            .AsNoTracking()
            .OrderBy(s => s.OrderHint)
            .ThenBy(s => s.Name)
            .Select(s => new WorkflowStageLibraryItemDto(
                s.Id, s.Name, s.Description, s.LevelRole,
                s.DefaultSlaHours, s.DefaultEscalationHours, s.DefaultAlgorithm,
                s.IsSystemStage, s.OrderHint))
            .ToListAsync(cancellationToken);
    }
}

// ─── POST: Create Stage Library Item ─────────────────────────────────────────

public record CreateStageLibraryItemCommand(
    string Name,
    string? Description,
    LevelRole LevelRole,
    int? DefaultSlaHours,
    int? DefaultEscalationHours,
    AssignmentAlgorithm DefaultAlgorithm,
    int OrderHint = 0
) : IRequest<WorkflowStageLibraryItemDto>;

public class CreateStageLibraryItemCommandHandler : IRequestHandler<CreateStageLibraryItemCommand, WorkflowStageLibraryItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateStageLibraryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowStageLibraryItemDto> Handle(CreateStageLibraryItemCommand request, CancellationToken cancellationToken)
    {
        var item = new WorkflowStageLibraryItem(
            request.Name,
            request.Description,
            request.LevelRole,
            request.DefaultSlaHours,
            request.DefaultEscalationHours,
            request.DefaultAlgorithm,
            isSystemStage: false, // user-created items are never system stages
            request.OrderHint);

        _context.WorkflowStageLibrary.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkflowStageLibraryItemDto(
            item.Id, item.Name, item.Description, item.LevelRole,
            item.DefaultSlaHours, item.DefaultEscalationHours, item.DefaultAlgorithm,
            item.IsSystemStage, item.OrderHint);
    }
}

// ─── PUT: Update Stage Library Item ──────────────────────────────────────────

public record UpdateStageLibraryItemCommand(
    Guid Id,
    string Name,
    string? Description,
    LevelRole LevelRole,
    int? DefaultSlaHours,
    int? DefaultEscalationHours,
    AssignmentAlgorithm DefaultAlgorithm,
    int OrderHint = 0
) : IRequest<WorkflowStageLibraryItemDto>;

public class UpdateStageLibraryItemCommandHandler : IRequestHandler<UpdateStageLibraryItemCommand, WorkflowStageLibraryItemDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStageLibraryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowStageLibraryItemDto> Handle(UpdateStageLibraryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.WorkflowStageLibrary
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (item == null) throw new NotFoundException(nameof(WorkflowStageLibraryItem), request.Id);

        item.Name = request.Name;
        item.Description = request.Description;
        item.LevelRole = request.LevelRole;
        item.DefaultSlaHours = request.DefaultSlaHours;
        item.DefaultEscalationHours = request.DefaultEscalationHours;
        item.DefaultAlgorithm = request.DefaultAlgorithm;
        item.OrderHint = request.OrderHint;

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkflowStageLibraryItemDto(
            item.Id, item.Name, item.Description, item.LevelRole,
            item.DefaultSlaHours, item.DefaultEscalationHours, item.DefaultAlgorithm,
            item.IsSystemStage, item.OrderHint);
    }
}

// ─── DELETE: Remove Stage Library Item ────────────────────────────────────────

public record DeleteStageLibraryItemCommand(Guid Id) : IRequest<bool>;

public class DeleteStageLibraryItemCommandHandler : IRequestHandler<DeleteStageLibraryItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStageLibraryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStageLibraryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.WorkflowStageLibrary
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (item == null) throw new NotFoundException(nameof(WorkflowStageLibraryItem), request.Id);

        if (item.IsSystemStage)
            throw new DomainException("System stages cannot be deleted. They are part of the approved Tax Ombud workflow and are required for the system to function correctly.");

        _context.WorkflowStageLibrary.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
