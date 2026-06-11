using MediatR;

namespace TaxOmbud.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}
