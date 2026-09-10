using System;
using System.Collections.Generic;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Constants;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Events.Cases;
using TaxOmbud.Domain.Entities.Workflows;

namespace TaxOmbud.Domain.Entities.Cases;

public class Case : BaseEntity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public ReferenceNumber CaseNumber { get; private set; } = null!;

    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public string Subject { get; set; } = null!;
    public string? Summary { get; set; }
    public string Priority { get; set; } = "medium"; // low, medium, high, urgent

    public CaseStatus Status { get; private set; } = CaseStatus.Submitted;

    /// <summary>
    /// Current workflow stage slug. Always one of the WorkflowStage constants.
    /// </summary>
    public string CurrentStage { get; private set; } = WorkflowStage.Intake;

    /// <summary>How the complaint was originally received.</summary>
    public IntakeChannel IntakeChannel { get; set; } = IntakeChannel.OnlinePortal;

    public Guid? AssignedOfficerId { get; private set; }
    public Officer? AssignedOfficer { get; private set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid AccountId { get; set; } // Workflow Lane (regional scope)
    public Account Account { get; set; } = null!;

    public DateTimeOffset? DueDate { get; set; } // SLA deadline
    public DateTimeOffset? ClosedAt { get; private set; }

    public string? Outcome { get; private set; }
    public string? FindingsSummary { get; private set; }

    /// <summary>
    /// Archiving fields — set at Stage 7 (Closure & Archiving).
    /// </summary>
    public bool IsArchived { get; private set; } = false;
    public DateTimeOffset? ArchivedAt { get; private set; }

    public int? CsatRating { get; set; }
    public int? NpsScore { get; set; }
    public string? CsatComment { get; set; }

    public Guid? ActiveWorkflowInstanceId { get; set; }
    public WorkflowInstance? ActiveWorkflowInstance { get; set; }

    public AdmissibilityAssessment? AdmissibilityAssessment { get; set; }
    public NotAdmissibleDecision? NotAdmissibleDecision { get; set; }
    public CaseArchiveRecord? ArchiveRecord { get; set; }

    public ICollection<MediationLog> MediationLogs { get; set; } = new List<MediationLog>();
    public ICollection<QualityAssuranceReview> QualityAssuranceReviews { get; set; } = new List<QualityAssuranceReview>();
    public CaseDecision? Decision { get; set; }

    public ICollection<CaseFinding> Findings { get; set; } = new List<CaseFinding>();
    public ICollection<CaseRecommendation> Recommendations { get; set; } = new List<CaseRecommendation>();
    public ICollection<CaseMilestone> Milestones { get; set; } = new List<CaseMilestone>();
    public ICollection<CaseCommunicationLog> CommunicationLogs { get; set; } = new List<CaseCommunicationLog>();
    public ICollection<CaseStatusHistory> StatusHistory { get; set; } = new List<CaseStatusHistory>();
    public ICollection<CaseWorkflowAuditLog> AuditLogs { get; set; } = new List<CaseWorkflowAuditLog>();

    // Constructor for EF Core
    protected Case() { }

    public Case(Guid complaintId, string subject, Guid accountId, string priority, IntakeChannel intakeChannel = IntakeChannel.OnlinePortal)
    {
        Id = Guid.NewGuid();
        ComplaintId = complaintId;
        Subject = subject;
        AccountId = accountId;
        Priority = priority;
        IntakeChannel = intakeChannel;
        Status = CaseStatus.Submitted;
        CurrentStage = WorkflowStage.Intake;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Stage 2 — Registration & Acknowledgement.
    /// Assigns the Case Reference Number and raises CaseOpenedEvent.
    /// </summary>
    public void Open(ReferenceNumber caseNumber)
    {
        if (CaseNumber != null)
            throw new DomainException("Case number has already been assigned.");

        CaseNumber = caseNumber;
        Status = CaseStatus.Registered;
        CurrentStage = WorkflowStage.RegistrationAndAcknowledgement;

        AddDomainEvent(new CaseOpenedEvent(Id, CaseNumber.Value, ComplaintId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 3 — Initial Review & Assignment.
    /// CE performs initial review and assigns to an officer.
    /// </summary>
    public void StartInitialReview()
    {
        Status = CaseStatus.UnderAssessment;
        CurrentStage = WorkflowStage.InitialReviewAndAssignment;
    }

    /// <summary>
    /// Stage 3 → 4 — CE assigns the case to an officer, transitioning to Jurisdiction & Admissibility Assessment.
    /// </summary>
    public void Assign(Guid officerId, Guid assignedByUserId)
    {
        if (Status == CaseStatus.Closed)
            throw new DomainException("Cannot assign an officer to a closed case.");

        AssignedOfficerId = officerId;
        Status = CaseStatus.Assigned;
        CurrentStage = WorkflowStage.JurisdictionAndAdmissibility;

        AddDomainEvent(new CaseAssignedEvent(Id, officerId, assignedByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Stage 5 — Investigation & Resolution.
    /// Called after the case is declared admissible at Stage 4.
    /// </summary>
    public void StartInvestigation()
    {
        if (Status == CaseStatus.Closed)
            throw new DomainException("Cannot start investigation on a closed case.");

        Status = CaseStatus.UnderInvestigation;
        CurrentStage = WorkflowStage.InvestigationAndResolution;
    }

    /// <summary>
    /// Stage 6 — Decision & Communication.
    /// CE issues a formal decision; triggers Decision Letter generation.
    /// </summary>
    public void IssueDecision(CaseDecision decision)
    {
        Decision = decision;
        Status = CaseStatus.DecisionIssued;
        CurrentStage = WorkflowStage.DecisionAndCommunication;
    }

    /// <summary>
    /// Stage 7 — Closure & Archiving.
    /// Formally closes the case and marks it as archived.
    /// </summary>
    public void Close(string outcome, string findingsSummary, Guid closedByUserId)
    {
        if (Status == CaseStatus.Closed)
            throw new DomainException("Case is already closed.");

        Status = CaseStatus.Closed;
        CurrentStage = WorkflowStage.ClosureAndArchiving;
        ClosedAt = DateTimeOffset.UtcNow;
        Outcome = outcome;
        FindingsSummary = findingsSummary;

        AddDomainEvent(new CaseClosedEvent(Id, outcome, closedByUserId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Marks the case as fully archived at Stage 7.
    /// Called after a CaseArchiveRecord has been created.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        ArchivedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Generic status update used by the workflow engine.
    /// Always supply one of the WorkflowStage constants as the stage argument.
    /// </summary>
    public void UpdateStatus(CaseStatus newStatus, string stage, Guid changedByUserId)
    {
        if (Status == CaseStatus.Closed)
            throw new DomainException("Cannot change status of a closed case.");

        Status = newStatus;
        CurrentStage = string.IsNullOrEmpty(stage) ? WorkflowStage.Intake : (stage.Length > 50 ? stage[..50] : stage);
    }
}