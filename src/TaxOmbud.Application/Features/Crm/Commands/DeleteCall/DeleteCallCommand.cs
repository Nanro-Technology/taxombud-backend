using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.DeleteCall;

public record DeleteCallCommand(Guid Id) : IRequest;

public class DeleteCallCommandHandler : IRequestHandler<DeleteCallCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCallCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calls.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Call), request.Id);
        }

        _context.Calls.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
