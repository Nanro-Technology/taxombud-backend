using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Events.Complaints;

public record ComplaintSubmittedEvent(Guid ComplaintId, string ReferenceNumber, Guid TaxpayerId, DateTimeOffset OccurredAt) : IDomainEvent;
