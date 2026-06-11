using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Communications;

namespace TaxOmbud.Application.Features.Communications.Commands.SendCommunication;

public record SendCommunicationCommand(Guid CommunicationId) : IRequest<Result<Unit>>;

public class SendCommunicationCommandHandler : IRequestHandler<SendCommunicationCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public SendCommunicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(SendCommunicationCommand request, CancellationToken cancellationToken)
    {
        var communication = await _context.CommunicationLogs
            .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

        if (communication == null)
            throw new NotFoundException(nameof(Domain.Entities.Communications.Communication), request.CommunicationId);

        if (communication.IsSent)
            return Result<Unit>.Failure("Communication has already been sent.");

        // In a real application, we would integrate with an email/SMS provider here.
        // For now, we just mark it as sent.
        communication.IsSent = true;
        communication.SentAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
