using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Tasks.DTOs;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Tasks.Queries.GetCaseTaskById;

public record GetCaseTaskByIdQuery(Guid Id) : IRequest<CaseTaskDto>;

public class GetCaseTaskByIdQueryHandler : IRequestHandler<GetCaseTaskByIdQuery, CaseTaskDto>
{
    private readonly IApplicationDbContext _context;

    public GetCaseTaskByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CaseTaskDto> Handle(GetCaseTaskByIdQuery request, CancellationToken cancellationToken)
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
}
