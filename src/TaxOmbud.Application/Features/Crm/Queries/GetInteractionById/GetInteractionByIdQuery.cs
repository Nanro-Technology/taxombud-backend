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

namespace TaxOmbud.Application.Features.Crm.Queries.GetInteractionById;

public record GetInteractionByIdQuery(Guid Id) : IRequest<InteractionDto>;

public class GetInteractionByIdQueryHandler : IRequestHandler<GetInteractionByIdQuery, InteractionDto>
{
    private readonly IApplicationDbContext _context;

    public GetInteractionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InteractionDto> Handle(GetInteractionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        return new InteractionDto
        {
            Id = entity.Id,
            Direction = entity.Direction,
            Subject = entity.Subject,
            Type = entity.Type,
            Channel = entity.Channel,
            Outcome = entity.Outcome,
            Notes = entity.Notes,
            RelatedToId = entity.RelatedToId,
            LoggedById = entity.LoggedById,
            OccurredAt = entity.OccurredAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }
}
