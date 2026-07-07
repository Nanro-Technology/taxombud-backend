using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Tasks.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Cases;

namespace TaxOmbud.Application.Services;

public class TasksService : ITasksService
{
    private readonly IGenericRepository<CaseTask> _taskRepo;

    public TasksService(IGenericRepository<CaseTask> taskRepo)
    {
        _taskRepo = taskRepo;
    }

    public async Task<Guid> CreateCaseTaskAsync(CreateCaseTaskCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new CaseTask
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueAt = request.DueAt,
            AssignedToId = request.AssignedToId,
            LinkedCaseId = request.LinkedCaseId
        };

        await _taskRepo.AddAsync(entity);
        await _taskRepo.SaveAsync();
        return entity.Id;
    }

    public async Task<DeleteCaseTaskCommand> DeleteCaseTaskAsync(DeleteCaseTaskCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _taskRepo.GetByIdAsync(request.Id);
        if (entity == null)
            throw new NotFoundException(nameof(CaseTask), request.Id);

        await _taskRepo.RemoveAsync(entity);
        await _taskRepo.SaveAsync();
        return request;
    }

    public async Task<UpdateCaseTaskCommand> UpdateCaseTaskAsync(UpdateCaseTaskCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _taskRepo.GetByIdAsync(request.Id);
        if (entity == null)
            throw new NotFoundException(nameof(CaseTask), request.Id);

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.Priority = request.Priority;
        entity.DueAt = request.DueAt;
        entity.AssignedToId = request.AssignedToId;
        entity.LinkedCaseId = request.LinkedCaseId;

        await _taskRepo.UpdateAsync(entity);
        await _taskRepo.SaveAsync();
        return request;
    }

    public async Task<CaseTaskDto> GetCaseTaskByIdAsync(GetCaseTaskByIdQuery request, CancellationToken cancellationToken = default)
    {
        var entity = await _taskRepo.FindAsync(x => x.Id == request.Id);
        if (entity == null)
            throw new NotFoundException(nameof(CaseTask), request.Id);

        return new CaseTaskDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            DueAt = entity.DueAt,
            AssignedToId = entity.AssignedToId,
            LinkedCaseId = entity.LinkedCaseId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedByUserId,
            UpdatedAt = entity.LastModifiedAt,
            UpdatedBy = entity.LastModifiedByUserId
        };
    }

    public async Task<List<CaseTaskDto>> GetCaseTasksAsync(GetCaseTasksQuery request, CancellationToken cancellationToken = default)
    {
        return await _taskRepo.Query()
            .AsNoTracking()
            .Select(x => new CaseTaskDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                DueAt = x.DueAt,
                AssignedToId = x.AssignedToId,
                LinkedCaseId = x.LinkedCaseId,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedByUserId,
                UpdatedAt = x.LastModifiedAt,
                UpdatedBy = x.LastModifiedByUserId
            })
            .ToListAsync(cancellationToken);
    }
}
