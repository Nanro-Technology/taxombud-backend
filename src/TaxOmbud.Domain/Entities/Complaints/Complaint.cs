using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Domain.Entities.Complaints;

public class Complaint : BaseEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public string ReferenceNumber { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? WhyOtoHandle { get; private set; }

    public Guid TaxpayerId { get; private set; }
    public TaxpayerProfile Taxpayer { get; private set; } = null!;

    public string TaxType { get; private set; } = null!;
    public string TaxPeriod { get; private set; } = null!;
    public string ComplaintCategory { get; private set; } = null!;
    public string? TaxOfficeRef { get; private set; }
    public string? TinNumber { get; private set; }

    public string Priority { get; private set; } = "medium";

    public ComplaintStatus Status { get; private set; } = ComplaintStatus.Draft;
    public string CurrentStage { get; private set; } = "input";

    public Guid? AssignedOfficerId { get; private set; }
    public Officer? AssignedOfficer { get; private set; }

    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }

    public bool RequiresApprovalToClose { get; private set; } = true;
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? WithdrawalReason { get; private set; }
    public string? ClosureReason { get; private set; }

    public ICollection<ComplaintStatusHistory> StatusHistory { get; private set; } = new List<ComplaintStatusHistory>();
    public ICollection<ComplaintNote> Notes { get; private set; } = new List<ComplaintNote>();
    public ICollection<ComplaintLink> Links { get; private set; } = new List<ComplaintLink>();
    public ICollection<CallCenterRecord> CallCenterRecords { get; private set; } = new List<CallCenterRecord>();

    protected Complaint() { }

    public static Complaint Create(
        Guid taxpayerId,
        string taxType,
        string taxPeriod,
        string category,
        string subject,
        string description,
        string referenceNumber,
        string? taxOfficeRef = null,
        string? tinNumber = null,
        string? whyOtoHandle = null)
    {
        return new Complaint
        {
            Id = Guid.NewGuid(),
            TaxpayerId = taxpayerId,
            TaxType = taxType,
            TaxPeriod = taxPeriod,
            ComplaintCategory = category,
            Subject = subject,
            Description = description,
            ReferenceNumber = referenceNumber,
            TaxOfficeRef = taxOfficeRef != null && taxOfficeRef.Length > 100 ? taxOfficeRef[..100] : taxOfficeRef,
            TinNumber = tinNumber != null && tinNumber.Length > 50 ? tinNumber[..50] : tinNumber,
            WhyOtoHandle = whyOtoHandle != null && whyOtoHandle.Length > 2000 ? whyOtoHandle[..2000] : whyOtoHandle,
            Status = ComplaintStatus.Draft,
            CurrentStage = "input"
        };
    }

    public void Submit()
    {
        if (Status != ComplaintStatus.Draft)
            throw new DomainException("Only complaints in Draft status can be submitted.");

        Status = ComplaintStatus.Submitted;
        CurrentStage = "verify";

        AddDomainEvent(new ComplaintSubmittedEvent(Id, ReferenceNumber, TaxpayerId, DateTimeOffset.UtcNow));
    }

    public void Assign(Guid officerId, Guid assignedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn)
            throw new DomainException("Cannot assign a closed or withdrawn complaint.");

        var previous = Status;
        AssignedOfficerId = officerId;

        if (Status == ComplaintStatus.Submitted)
        {
            Status = ComplaintStatus.Assigned;
            CurrentStage = "4_assignment";
        }

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, assignedByUserId, DateTimeOffset.UtcNow));
    }

    public void Escalate(string reason, Guid escalatedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn)
            throw new DomainException("Cannot escalate a complaint that is already closed or withdrawn.");

        if (Status == ComplaintStatus.UnderInvestigation)
            throw new DomainException("Complaint is already under investigation and cannot be re-escalated.");

        var previous = Status;
        Status = ComplaintStatus.UnderInvestigation;
        CurrentStage = "5_investigation";

        AddDomainEvent(new ComplaintEscalatedEvent(Id, previous, reason, escalatedByUserId, DateTimeOffset.UtcNow));
    }

    public void Close(string reason, Guid closedByUserId)
    {
        if (Status == ComplaintStatus.Closed)
            throw new DomainException("Complaint is already closed.");

        var previous = Status;
        Status = ComplaintStatus.Closed;
        CurrentStage = "10_closure";
        ClosedAt = DateTimeOffset.UtcNow;
        ClosureReason = reason;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, closedByUserId, DateTimeOffset.UtcNow));
    }

    public void Reopen(Guid reopenedByUserId)
    {
        if (Status != ComplaintStatus.Closed)
            throw new DomainException("Only closed complaints can be reopened.");

        Status = ComplaintStatus.UnderAssessment;
        CurrentStage = "3_assessment";
        ClosedAt = null;
        ClosureReason = null;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, ComplaintStatus.Closed, Status, reopenedByUserId, DateTimeOffset.UtcNow));
    }

    public void Withdraw(string reason, Guid taxpayerUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn)
            throw new DomainException("Complaint is already closed or withdrawn.");

        var previous = Status;
        Status = ComplaintStatus.Withdrawn;
        CurrentStage = "10_closure";
        ClosedAt = DateTimeOffset.UtcNow;
        WithdrawalReason = reason;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, taxpayerUserId, DateTimeOffset.UtcNow));
    }

    public void Resolve(Guid resolvedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn)
            throw new DomainException("Cannot resolve a closed or withdrawn complaint.");

        var previous = Status;
        Status = ComplaintStatus.DecisionIssued;
        CurrentStage = "9_decision";

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, resolvedByUserId, DateTimeOffset.UtcNow));
    }

    public void UpdateStatus(ComplaintStatus newStatus, string stage)
    {
        Status = newStatus;
        CurrentStage = string.IsNullOrEmpty(stage) ? "input" : (stage.Length > 50 ? stage.Substring(0, 50) : stage);
    }

    public void UpdateStatus(CaseStatus caseStatus, string stage)
    {
        if (Enum.TryParse<ComplaintStatus>(caseStatus.ToString(), true, out var mapped))
        {
            Status = mapped;
        }
        CurrentStage = string.IsNullOrEmpty(stage) ? "input" : (stage.Length > 50 ? stage.Substring(0, 50) : stage);
    }

    public void UpdatePriority(string priority) => Priority = priority;
    public void UpdateStage(string stage) => CurrentStage = string.IsNullOrEmpty(stage) ? "input" : (stage.Length > 50 ? stage.Substring(0, 50) : stage);
    public void SetDepartment(Guid departmentId) => DepartmentId = departmentId;

    public void UpdateDetails(
        string subject, string description, string taxType, string taxPeriod,
        string category, string? taxOfficeRef, string? tinNumber)
    {
        Subject = subject;
        Description = description;
        TaxType = taxType;
        TaxPeriod = taxPeriod;
        ComplaintCategory = category;
        TaxOfficeRef = taxOfficeRef;
        TinNumber = tinNumber;
    }
}