using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Application.EventHandlers;

public class ComplaintStatusChangedEventHandler : INotificationHandler<ComplaintStatusChangedEvent>
{
    private readonly IApplicationDbContext _context;

    public ComplaintStatusChangedEventHandler(IApplicationDbContext context)
    {
        _context = context;
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

        _context.ComplaintStatusHistory.Add(history);
        // SaveChangesAsync is called right after event dispatch in ApplicationDbContext, 
        // so we don't need to call SaveChangesAsync here.
        await Task.CompletedTask;
    }
}
