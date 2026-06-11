using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Appeals;

public record AppealSubmittedEvent(
    Guid AppealId,
    Guid CaseId,
    Guid SubmittedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
