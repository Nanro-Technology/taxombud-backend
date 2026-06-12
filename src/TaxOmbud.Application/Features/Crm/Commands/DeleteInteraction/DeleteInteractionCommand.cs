using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.DeleteInteraction;

public record DeleteInteractionCommand(Guid Id) : IRequest;

public class DeleteInteractionCommandHandler : IRequestHandler<DeleteInteractionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteInteractionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteInteractionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Interactions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        _context.Interactions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
