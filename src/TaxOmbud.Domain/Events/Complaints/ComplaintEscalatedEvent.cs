using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Events.Complaints;

public record ComplaintEscalatedEvent(
    Guid ComplaintId,
    ComplaintStatus PreviousStatus,
    string Reason,
    Guid EscalatedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;