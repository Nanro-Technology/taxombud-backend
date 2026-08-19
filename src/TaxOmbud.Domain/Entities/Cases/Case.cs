using System;
using System.Collections.Generic;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Common;
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
    public string CurrentStage { get; private set; } = "1_submission";
    public string? CurrentSubStage { get; private set; }  // e.g. "6_mediation", "7_findings", "8_qa_review"

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
    
    public int? CsatRating { get; set; }
    public int? NpsScore { get; set; }
    public string? CsatComment { get; set; }

    public Guid? ActiveWorkflowInstanceId { get; set; }
    public WorkflowInstance? ActiveWorkflowInstance { get; set; }

    public AdmissibilityAssessment? AdmissibilityAssessment { get; set; }
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

    public Case(Guid complaintId, string subject, Guid accountId, string priority)
    {
        Id = Guid.NewGuid();
        ComplaintId = complaintId;
        Subject = subject;
        AccountId = accountId;
        Priority = priority;
        Status = CaseStatus.Submitted;
        CurrentStage = "1_submission";
        CreatedAt = DateTime.UtcNow;
    }

    public void Open(ReferenceNumber caseNumber)
    {
        if (CaseNumber != null)
        {
            throw new DomainException("Case number has already been assigned.");
        }

        CaseNumber = caseNumber;
        Status = CaseStatus.Registered;
        CurrentStage = "2_registration";
        
        AddDomainEvent(new CaseOpenedEvent(Id, CaseNumber.Value, ComplaintId, DateTimeOffset.UtcNow));
    }

    public void MoveToAssessment()
    {
        Status = CaseStatus.UnderAssessment;
        CurrentStage = "3_assessment";
    }

    public void Assign(Guid officerId, Guid assignedByUserId)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Cannot assign an officer to a closed case.");
        }

        AssignedOfficerId = officerId;
        Status = CaseStatus.Assigned;
        CurrentStage = "4_assignment";

        AddDomainEvent(new CaseAssignedEvent(Id, officerId, assignedByUserId, DateTimeOffset.UtcNow));
    }

    public void StartInvestigation()
    {
        if (Status == CaseStatus.Closed)
            throw new DomainException("Cannot start investigation on a closed case.");

        if (Status != CaseStatus.Assigned)
            throw new DomainException("A case must be assigned to an officer before investigation can begin.");

        Status = CaseStatus.UnderInvestigation;
        CurrentStage = "5_investigation";
        CurrentSubStage = "5_investigation";
    }

    public void SetSubStage(string subStage)
        => CurrentSubStage = subStage;

    public void IssueDecision(CaseDecision decision)
    {
        Decision = decision;
        Status = CaseStatus.DecisionIssued;
        CurrentStage = "9_decision";
    }

    public void UpdateStatus(CaseStatus newStatus, string stage, Guid changedByUserId)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Cannot change status of a closed case.");
        }

        Status = newStatus;
        CurrentStage = string.IsNullOrEmpty(stage) ? "1_submission" : (stage.Length > 50 ? stage.Substring(0, 50) : stage);
    }

    public void Close(string outcome, string findingsSummary, Guid closedByUserId)
    {
        if (Status == CaseStatus.Closed)
        {
            throw new DomainException("Case is already closed.");
        }

        Status = CaseStatus.Closed;
        CurrentStage = "10_closure";
        ClosedAt = DateTimeOffset.UtcNow;
        Outcome = outcome;
        FindingsSummary = findingsSummary;

        AddDomainEvent(new CaseClosedEvent(Id, outcome, closedByUserId, DateTimeOffset.UtcNow));
    }
}