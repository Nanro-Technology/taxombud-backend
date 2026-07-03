using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Events.Appeals;

namespace TaxOmbud.Domain.Entities.Appeals;

public class Appeal : BaseEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public string Reason { get; set; } = null!;
    
    public AppealStatus Status { get; private set; } = AppealStatus.Submitted;

    public Guid? ReviewedByUserId { get; private set; }
    public string? ReviewNote { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    public ICollection<AppealStatusHistory> StatusHistory { get; set; } = new List<AppealStatusHistory>();

    // Constructor for EF Core
    protected Appeal() { }

    public Appeal(Guid caseId, string reason)
    {
        Id = Guid.NewGuid();
        CaseId = caseId;
        Reason = reason;
        Status = AppealStatus.Submitted;
        CreatedAt = DateTime.UtcNow;
    }

    public void Submit(Guid submittedByUserId)
    {
        AddDomainEvent(new AppealSubmittedEvent(Id, CaseId, submittedByUserId, DateTimeOffset.UtcNow));
    }

    public void Review(Guid reviewerUserId, string notes)
    {
        if (Status != AppealStatus.Submitted)
        {
            throw new DomainException("Appeal is already reviewed or under review.");
        }

        Status = AppealStatus.UnderReview;
        ReviewedByUserId = reviewerUserId;
        ReviewNote = notes;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public void Uphold(Guid reviewerUserId, string notes)
    {
        if (Status != AppealStatus.UnderReview && Status != AppealStatus.Submitted)
        {
            throw new DomainException("Appeal must be submitted or under review to be upheld.");
        }

        Status = AppealStatus.Upheld;
        ReviewedByUserId = reviewerUserId;
        ReviewNote = notes;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public void Dismiss(Guid reviewerUserId, string notes)
    {
        if (Status != AppealStatus.UnderReview && Status != AppealStatus.Submitted)
        {
            throw new DomainException("Appeal must be submitted or under review to be dismissed.");
        }

        Status = AppealStatus.Dismissed;
        ReviewedByUserId = reviewerUserId;
        ReviewNote = notes;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}
