using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Cases;

public record CaseOpenedEvent(Guid CaseId, string CaseNumber, Guid ComplaintId, DateTimeOffset OccurredAt) : IDomainEvent;
