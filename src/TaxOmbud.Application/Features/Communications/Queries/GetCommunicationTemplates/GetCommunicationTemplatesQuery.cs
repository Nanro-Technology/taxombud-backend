using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Queries.GetCommunicationTemplates;

public record GetCommunicationTemplatesQuery() : IRequest<Result<List<CommunicationTemplateDto>>>;

public record CommunicationTemplateDto(
    Guid Id,
    string Name,
    string SubjectTemplate,
    string BodyTemplate,
    string Channel,
    bool IsActive
);

public class GetCommunicationTemplatesQueryHandler : IRequestHandler<GetCommunicationTemplatesQuery, Result<List<CommunicationTemplateDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CommunicationTemplateDto>>> Handle(GetCommunicationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _context.CommunicationTemplates
            .AsNoTracking()
            .Select(t => new CommunicationTemplateDto(
                t.Id,
                t.Name,
                t.SubjectTemplate,
                t.BodyTemplate,
                t.Channel,
                t.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<CommunicationTemplateDto>>.Success(templates);
    }
}
