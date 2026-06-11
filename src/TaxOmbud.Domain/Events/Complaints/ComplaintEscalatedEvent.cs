using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Complaints;

public record ComplaintEscalatedEvent(
    Guid ComplaintId,
    string Reason,
    Guid EscalatedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
