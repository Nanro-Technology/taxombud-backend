using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Cases;

public record CaseAssignedEvent(
    Guid CaseId,
    Guid AssignedOfficerId,
    Guid AssignedByUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
