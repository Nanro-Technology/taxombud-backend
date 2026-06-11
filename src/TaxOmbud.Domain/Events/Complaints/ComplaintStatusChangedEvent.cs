using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Events.Complaints;

public record ComplaintStatusChangedEvent(
    Guid ComplaintId,
    ComplaintStatus OldStatus,
    ComplaintStatus NewStatus,
    Guid ChangedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
