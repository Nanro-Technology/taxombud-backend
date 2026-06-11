using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Cases;

public record CaseClosedEvent(
    Guid CaseId,
    string ClosureReason,
    Guid ClosedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
