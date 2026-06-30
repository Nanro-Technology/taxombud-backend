using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Tasks.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;
using FluentValidation;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Services;

public class TasksService : ITasksService
{
    private readonly IApplicationDbContext _context;

    public TasksService(
        IApplicationDbContext context
    )
    {
        _context = context;
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

        _context.CaseTasks.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<DeleteCaseTaskCommand> DeleteCaseTaskAsync(DeleteCaseTaskCommand request, CancellationToken cancellationToken = default)
{
        var entity = await _context.CaseTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CaseTask), request.Id);
        }

        _context.CaseTasks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<UpdateCaseTaskCommand> UpdateCaseTaskAsync(UpdateCaseTaskCommand request, CancellationToken cancellationToken = default)
{
        var entity = await _context.CaseTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CaseTask), request.Id);
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.Priority = request.Priority;
        entity.DueAt = request.DueAt;
        entity.AssignedToId = request.AssignedToId;
        entity.LinkedCaseId = request.LinkedCaseId;

        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<CaseTaskDto> GetCaseTaskByIdAsync(GetCaseTaskByIdQuery request, CancellationToken cancellationToken = default)
{
        var entity = await _context.CaseTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CaseTask), request.Id);
        }

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
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public async Task<List<CaseTaskDto>> GetCaseTasksAsync(GetCaseTasksQuery request, CancellationToken cancellationToken = default)
{
        return await _context.CaseTasks
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
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }

}