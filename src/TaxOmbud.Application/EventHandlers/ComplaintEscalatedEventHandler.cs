using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Application.EventHandlers;

public class ComplaintEscalatedEventHandler : INotificationHandler<ComplaintEscalatedEvent>
{
    private readonly IGenericRepository<ComplaintStatusHistory> _historyRepo;

    public ComplaintEscalatedEventHandler(IGenericRepository<ComplaintStatusHistory> historyRepo)
    {
        _historyRepo = historyRepo;
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

        await _historyRepo.AddAsync(history);
        await Task.CompletedTask;
    }
}
