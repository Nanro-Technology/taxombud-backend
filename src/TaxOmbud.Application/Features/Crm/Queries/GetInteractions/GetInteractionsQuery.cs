using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Features.Crm.DTOs;

namespace TaxOmbud.Application.Features.Crm.Queries.GetInteractions;

public record GetInteractionsQuery : IRequest<List<InteractionDto>>;

public class GetInteractionsQueryHandler : IRequestHandler<GetInteractionsQuery, List<InteractionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInteractionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InteractionDto>> Handle(GetInteractionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Interactions
            .AsNoTracking()
            .Select(x => new InteractionDto
            {
                Id = x.Id,
                Direction = x.Direction,
                Subject = x.Subject,
                Type = x.Type,
                Channel = x.Channel,
                Outcome = x.Outcome,
                Notes = x.Notes,
                RelatedToId = x.RelatedToId,
                LoggedById = x.LoggedById,
                OccurredAt = x.OccurredAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
