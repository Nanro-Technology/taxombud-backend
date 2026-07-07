using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Application.EventHandlers;

public class ComplaintStatusChangedEventHandler : INotificationHandler<ComplaintStatusChangedEvent>
{
    private readonly IGenericRepository<ComplaintStatusHistory> _historyRepo;

    public ComplaintStatusChangedEventHandler(IGenericRepository<ComplaintStatusHistory> historyRepo)
    {
        _historyRepo = historyRepo;
    }

    public async Task Handle(ComplaintStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var history = new ComplaintStatusHistory
        {
            Id = Guid.NewGuid(),
            ComplaintId = notification.ComplaintId,
            OldStatus = notification.OldStatus,
            NewStatus = notification.NewStatus,
            ChangedByUserId = notification.ChangedByUserId,
            TransitionedAt = notification.OccurredAt,
            Reason = $"Transitioned from {notification.OldStatus} to {notification.NewStatus}."
        };

        await _historyRepo.AddAsync(history);
        // SaveChangesAsync is called right after event dispatch in ApplicationDbContext, 
        // so we don't need to call SaveChangesAsync here.
        await Task.CompletedTask;
    }
}
