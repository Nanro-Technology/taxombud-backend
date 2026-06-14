using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Crm.DTOs;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Queries.GetCallById;

public record GetCallByIdQuery(Guid Id) : IRequest<CallDto>;

public class GetCallByIdQueryHandler : IRequestHandler<GetCallByIdQuery, CallDto>
{
    private readonly IApplicationDbContext _context;

    public GetCallByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CallDto> Handle(GetCallByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Call), request.Id);
        }

        return new CallDto
        {
            Id = entity.Id,
            Subject = entity.Subject,
            CallerType = entity.CallerType,
            CallerMethod = entity.CallerMethod,
            CallerIdentifier = entity.CallerIdentifier,
            CalleeMethod = entity.CalleeMethod,
            CalleeIdentifier = entity.CalleeIdentifier,
            Direction = entity.Direction,
            Status = entity.Status,
            Phone = entity.Phone,
            Notes = entity.Notes,
            LinkedToId = entity.LinkedToId,
            AgentId = entity.AgentId,
            StartAt = entity.StartAt,
            EndAt = entity.EndAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }
}
