using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.RenderCommunicationTemplate;

public record RenderCommunicationTemplateCommand(Guid TemplateId, Dictionary<string, string> Payload) : IRequest<Result<RenderedTemplateDto>>;

public record RenderedTemplateDto(string Subject, string Body);

public class RenderCommunicationTemplateCommandHandler : IRequestHandler<RenderCommunicationTemplateCommand, Result<RenderedTemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public RenderCommunicationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RenderedTemplateDto>> Handle(RenderCommunicationTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.CommunicationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
            throw new NotFoundException(nameof(Domain.Entities.Communications.CommunicationTemplate), request.TemplateId);

        var subject = template.SubjectTemplate;
        var body = template.BodyTemplate;

        if (request.Payload != null)
        {
            foreach (var kvp in request.Payload)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}"; // e.g. {{Name}}
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
        }

        return Result<RenderedTemplateDto>.Success(new RenderedTemplateDto(subject, body));
    }
}
