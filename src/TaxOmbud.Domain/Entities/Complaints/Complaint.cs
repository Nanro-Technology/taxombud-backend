using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Constants;
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

    /// <summary>
    /// Current workflow stage slug. Always one of the WorkflowStage constants.
    /// </summary>
    public string CurrentStage { get; private set; } = WorkflowStage.Intake;

    /// <summary>How the complaint was originally received.</summary>
    public IntakeChannel IntakeChannel { get; set; } = IntakeChannel.OnlinePortal;

    public Guid? AssignedOfficerId { get; private set; }
    public Officer? AssignedOfficer { get; private set; }

    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }

    public bool RequiresApprovalToClose { get; private set; } = true;
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? WithdrawalReason { get; private set; }
    public string? ClosureReason { get; private set; }

    /// <summary>Reason provided when a complaint is declared Not Admissible at Stage 4.</summary>
    public string? NotAdmissibleReason { get; private set; }

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
        IntakeChannel intakeChannel = IntakeChannel.OnlinePortal,
        string? taxOfficeRef = null,
        string? tinNumber = null,
        string? whyOtoHandle = null)
    {
        return new Complaint
        {
            Id = Guid.NewGuid(),
            TaxpayerId = taxpayerId,
            TaxType = taxType != null && taxType.Length > 50 ? taxType[..50] : (taxType ?? "Tax Dispute"),
            TaxPeriod = taxPeriod != null && taxPeriod.Length > 50 ? taxPeriod[..50] : (taxPeriod ?? DateTimeOffset.UtcNow.Year.ToString()),
            ComplaintCategory = category != null && category.Length > 100 ? category[..100] : (category ?? "General"),
            Subject = subject != null && subject.Length > 500 ? subject[..500] : (subject ?? "Tax Complaint"),
            Description = description != null && description.Length > 5000 ? description[..5000] : (description ?? ""),
            ReferenceNumber = referenceNumber,
            IntakeChannel = intakeChannel,
            TaxOfficeRef = taxOfficeRef != null && taxOfficeRef.Length > 100 ? taxOfficeRef[..100] : taxOfficeRef,
            TinNumber = tinNumber != null && tinNumber.Length > 50 ? tinNumber[..50] : tinNumber,
            WhyOtoHandle = whyOtoHandle != null && whyOtoHandle.Length > 2000 ? whyOtoHandle[..2000] : whyOtoHandle,
            Status = ComplaintStatus.Draft,
            CurrentStage = WorkflowStage.Intake
        };
    }

    /// <summary>
    /// Stage 1 — Intake. Taxpayer submits the complaint.
    /// </summary>
    public void Submit()
    {
        if (Status != ComplaintStatus.Draft)
            throw new DomainException("Only complaints in Draft status can be submitted.");

        Status = ComplaintStatus.Submitted;
        CurrentStage = WorkflowStage.Intake;

        AddDomainEvent(new ComplaintSubmittedEvent(Id, ReferenceNumber, TaxpayerId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 3 → 4 — CE assigns the complaint to an investigating officer.
    /// </summary>
    public void Assign(Guid officerId, Guid assignedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn || Status == ComplaintStatus.NotAdmissible)
            throw new DomainException("Cannot assign a closed, withdrawn, or not-admissible complaint.");

        var previous = Status;
        AssignedOfficerId = officerId;

        if (Status == ComplaintStatus.Submitted || Status == ComplaintStatus.Registered || Status == ComplaintStatus.UnderAssessment)
        {
            Status = ComplaintStatus.Assigned;
            CurrentStage = WorkflowStage.JurisdictionAndAdmissibility;
        }

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, assignedByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 4 terminal — Declares the complaint NOT ADMISSIBLE.
    /// Sets status to NotAdmissible and records the reason. A formal notification email is dispatched by the service layer.
    /// </summary>
    public void DeclareNotAdmissible(string reason, Guid declaredByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn || Status == ComplaintStatus.NotAdmissible)
            throw new DomainException("This complaint cannot be declared not admissible in its current state.");

        var previous = Status;
        Status = ComplaintStatus.NotAdmissible;
        CurrentStage = WorkflowStage.NotAdmissible;
        ClosedAt = DateTimeOffset.UtcNow;
        NotAdmissibleReason = reason;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, declaredByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 5 — Investigation & Resolution.
    /// </summary>
    public void Escalate(string reason, Guid escalatedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn || Status == ComplaintStatus.NotAdmissible)
            throw new DomainException("Cannot escalate a complaint that is closed, withdrawn, or not admissible.");

        if (Status == ComplaintStatus.UnderInvestigation)
            throw new DomainException("Complaint is already under investigation and cannot be re-escalated.");

        var previous = Status;
        Status = ComplaintStatus.UnderInvestigation;
        CurrentStage = WorkflowStage.InvestigationAndResolution;

        AddDomainEvent(new ComplaintEscalatedEvent(Id, previous, reason, escalatedByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 7 — Closure & Archiving.
    /// </summary>
    public void Close(string reason, Guid closedByUserId)
    {
        if (Status == ComplaintStatus.Closed)
            throw new DomainException("Complaint is already closed.");

        var previous = Status;
        Status = ComplaintStatus.Closed;
        CurrentStage = WorkflowStage.ClosureAndArchiving;
        ClosedAt = DateTimeOffset.UtcNow;
        ClosureReason = reason;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, closedByUserId, DateTimeOffset.UtcNow));
    }

    public void Reopen(Guid reopenedByUserId)
    {
        if (Status != ComplaintStatus.Closed)
            throw new DomainException("Only closed complaints can be reopened.");

        Status = ComplaintStatus.UnderAssessment;
        CurrentStage = WorkflowStage.InitialReviewAndAssignment;
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
        CurrentStage = WorkflowStage.ClosureAndArchiving;
        ClosedAt = DateTimeOffset.UtcNow;
        WithdrawalReason = reason;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, taxpayerUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 6 — Decision & Communication.
    /// </summary>
    public void Resolve(Guid resolvedByUserId)
    {
        if (Status == ComplaintStatus.Closed || Status == ComplaintStatus.Withdrawn || Status == ComplaintStatus.NotAdmissible)
            throw new DomainException("Cannot resolve a closed, withdrawn, or not-admissible complaint.");

        var previous = Status;
        Status = ComplaintStatus.DecisionIssued;
        CurrentStage = WorkflowStage.DecisionAndCommunication;

        AddDomainEvent(new ComplaintStatusChangedEvent(Id, previous, Status, resolvedByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Generic status update used by the workflow engine.
    /// Always supply one of the WorkflowStage constants as the stage argument.
    /// </summary>
    public void UpdateStatus(ComplaintStatus newStatus, string stage)
    {
        Status = newStatus;
        CurrentStage = string.IsNullOrEmpty(stage) ? WorkflowStage.Intake : (stage.Length > 50 ? stage[..50] : stage);
    }

    /// <summary>
    /// Mirrors the Case's CaseStatus to keep Complaint in sync via the workflow engine.
    /// </summary>
    public void UpdateStatus(CaseStatus caseStatus, string stage)
    {
        if (Enum.TryParse<ComplaintStatus>(caseStatus.ToString(), true, out var mapped))
            Status = mapped;
        CurrentStage = string.IsNullOrEmpty(stage) ? WorkflowStage.Intake : (stage.Length > 50 ? stage[..50] : stage);
    }

    public void UpdatePriority(string priority) => Priority = priority;
    public void UpdateStage(string stage) => CurrentStage = string.IsNullOrEmpty(stage) ? WorkflowStage.Intake : (stage.Length > 50 ? stage[..50] : stage);
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