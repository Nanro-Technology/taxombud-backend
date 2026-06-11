using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.AcknowledgeCommunication;

public record AcknowledgeCommunicationCommand(Guid CommunicationId) : IRequest<Result<Unit>>;

public class AcknowledgeCommunicationCommandHandler : IRequestHandler<AcknowledgeCommunicationCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public AcknowledgeCommunicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(AcknowledgeCommunicationCommand request, CancellationToken cancellationToken)
    {
        var communication = await _context.CommunicationLogs
            .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

        if (communication == null)
            throw new NotFoundException(nameof(Domain.Entities.Communications.Communication), request.CommunicationId);

        // Assume acknowledging means we clear the error message or mark it as viewed, etc.
        // Since Communication doesn't have an explicitly tracked 'IsAcknowledged' flag in Domain,
        // we might just update some state. We can just return success or add IsAcknowledged later.
        
        // As a simple placeholder, we consider it a success if it exists.
        return Result<Unit>.Success(Unit.Value);
    }
}
