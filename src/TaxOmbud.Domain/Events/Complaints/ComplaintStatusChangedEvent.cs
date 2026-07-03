using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Complaints;

public record ComplaintStatusChangedEvent(
    Guid ComplaintId,
    ComplaintStatus OldStatus,
    ComplaintStatus NewStatus,
    Guid ChangedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
