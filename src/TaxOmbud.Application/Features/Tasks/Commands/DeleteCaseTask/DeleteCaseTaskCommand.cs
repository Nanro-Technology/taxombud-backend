using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Tasks.Commands.DeleteCaseTask;

public record DeleteCaseTaskCommand(Guid Id) : IRequest;

public class DeleteCaseTaskCommandHandler : IRequestHandler<DeleteCaseTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCaseTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCaseTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CaseTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CaseTask), request.Id);
        }

        _context.CaseTasks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
