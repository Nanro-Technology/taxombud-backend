using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Application.EventHandlers;

public class ComplaintEscalatedEventHandler : INotificationHandler<ComplaintEscalatedEvent>
{
    private readonly IApplicationDbContext _context;

    public ComplaintEscalatedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ComplaintEscalatedEvent notification, CancellationToken cancellationToken)
    {
        var history = new ComplaintStatusHistory
        {
            Id = Guid.NewGuid(),
            ComplaintId = notification.ComplaintId,
            OldStatus = ComplaintStatus.UnderReview, // Escalation only occurs from UnderReview
            NewStatus = ComplaintStatus.Escalated,
            ChangedByUserId = notification.EscalatedByUserId,
            TransitionedAt = notification.OccurredAt,
            Reason = notification.Reason
        };

        _context.ComplaintStatusHistory.Add(history);
        await Task.CompletedTask;
    }
}
