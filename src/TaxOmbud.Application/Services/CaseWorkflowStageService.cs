using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Services;

public class CaseWorkflowStageService : ICaseWorkflowStageService
{
    private readonly IApplicationDbContext _context;

    public CaseWorkflowStageService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RegisterComplaintAsync(Guid complaintId, Guid registeredBy)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == complaintId);
        if (complaint == null) return false;

        // Transition complaint & underlying case status to Registered
        var existingCase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
        if (existingCase != null)
        {
            existingCase.UpdateStatus(CaseStatus.Registered, "2_registration", registeredBy);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssessAdmissibilityAsync(Guid caseId, AdmissibilityAssessmentDto dto, Guid assessedBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.AdmissibilityAssessment)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseItem == null) return false;

        if (caseItem.AdmissibilityAssessment == null)
        {
            caseItem.AdmissibilityAssessment = new AdmissibilityAssessment
            {
                Id = Guid.NewGuid(),
                CaseId = caseId
            };
        }

        var assessment = caseItem.AdmissibilityAssessment;
        assessment.IsNotAnonymous = dto.IsNotAnonymous;
        assessment.IsNotInCourt = dto.IsNotInCourt;
        assessment.IsWithinMandate = dto.IsWithinMandate;
        assessment.HasSupportingDocuments = dto.HasSupportingDocuments;
        assessment.HasExhaustedInternalProcedures = dto.HasExhaustedInternalProcedures;
        assessment.IsAdmissible = dto.IsAdmissible;
        assessment.ScreeningNotes = dto.ScreeningNotes;
        assessment.RejectionReason = dto.RejectionReason;
        assessment.AssessedByUserId = assessedBy;
        assessment.AssessedAt = DateTimeOffset.UtcNow;

        if (dto.IsAdmissible)
        {
            caseItem.UpdateStatus(CaseStatus.UnderAssessment, "3_assessment", assessedBy);
        }
        else
        {
            caseItem.Close("Inadmissible - Rejected at Assessment", dto.RejectionReason ?? "Screening failed admissibility criteria.", assessedBy);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignCaseByCeAsync(Guid caseId, Guid officerId, Guid departmentId, Guid assignedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        if (caseItem == null) return false;

        caseItem.DepartmentId = departmentId;
        caseItem.Assign(officerId, assignedBy);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LogMediationSessionAsync(Guid caseId, MediationLogDto dto, Guid loggedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        if (caseItem == null) return false;

        var log = new MediationLog
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            SessionDate = dto.SessionDate,
            Attendees = dto.Attendees,
            SummaryOfDiscussions = dto.SummaryOfDiscussions,
            SettlementProposal = dto.SettlementProposal,
            IsAmicablySettled = dto.IsAmicablySettled,
            AgreementDocumentUrl = dto.AgreementDocumentUrl,
            LoggedByUserId = loggedBy,
            LoggedAt = DateTimeOffset.UtcNow
        };

        _context.MediationLogs.Add(log);
        caseItem.StartInvestigation();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SubmitQaReviewAsync(Guid caseId, QualityAssuranceReviewDto dto, Guid reviewedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        if (caseItem == null) return false;

        var qa = new QualityAssuranceReview
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            AccuracyVerified = dto.AccuracyVerified,
            ConsistencyVerified = dto.ConsistencyVerified,
            LegalComplianceVerified = dto.LegalComplianceVerified,
            PolicyAdherenceVerified = dto.PolicyAdherenceVerified,
            IsApprovedForDecision = dto.IsApprovedForDecision,
            QaComments = dto.QaComments,
            RevisionInstructions = dto.RevisionInstructions,
            ReviewedByUserId = reviewedBy,
            ReviewedAt = DateTimeOffset.UtcNow
        };

        _context.QualityAssuranceReviews.Add(qa);
        
        if (dto.IsApprovedForDecision)
        {
            caseItem.UpdateStatus(CaseStatus.UnderInvestigation, "8_qa_approved", reviewedBy);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IssueCeDecisionAsync(Guid caseId, CaseDecisionDto dto, Guid issuedBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.Decision)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseItem == null) return false;

        var decision = new CaseDecision
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            DecisionSummary = dto.DecisionSummary,
            LegalBasisCitations = dto.LegalBasisCitations,
            RecommendationsApproved = dto.RecommendationsApproved,
            DecisionDocumentUrl = dto.DecisionDocumentUrl,
            IssuerTitle = dto.IssuerTitle,
            IssuedByUserId = issuedBy,
            IssuedAt = DateTimeOffset.UtcNow
        };

        _context.CaseDecisions.Add(decision);
        caseItem.IssueDecision(decision);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseAndArchiveCaseAsync(Guid caseId, string outcome, string summary, Guid closedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        if (caseItem == null) return false;

        caseItem.Close(outcome, summary, closedBy);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Guid> LogCallCenterRecordAsync(CallCenterRecordDto dto, Guid loggedBy)
    {
        var record = new CallCenterRecord
        {
            Id = Guid.NewGuid(),
            ComplaintId = dto.ComplaintId,
            CallerName = dto.CallerName,
            CallerPhoneNumber = dto.CallerPhoneNumber,
            HotlineLineUsed = dto.HotlineLineUsed,
            DurationSeconds = dto.DurationSeconds,
            RecordingFileUrl = dto.RecordingFileUrl,
            CallSummary = dto.CallSummary,
            LoggedByAgentId = loggedBy,
            LoggedAt = DateTimeOffset.UtcNow
        };

        _context.CallCenterRecords.Add(record);
        await _context.SaveChangesAsync();
        return record.Id;
    }

    public async Task<WorkflowStageDetailsDto?> GetWorkflowStageDetailsAsync(Guid caseId)
    {
        var caseItem = await _context.Cases
            .Include(c => c.AdmissibilityAssessment)
            .Include(c => c.MediationLogs)
            .Include(c => c.QualityAssuranceReviews)
            .Include(c => c.Decision)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseItem == null) return null;

        var callRecords = await _context.CallCenterRecords
            .Where(r => r.ComplaintId == caseItem.ComplaintId)
            .Select(r => new CallCenterRecordDto
            {
                ComplaintId = r.ComplaintId,
                CallerName = r.CallerName,
                CallerPhoneNumber = r.CallerPhoneNumber,
                HotlineLineUsed = r.HotlineLineUsed,
                DurationSeconds = r.DurationSeconds,
                RecordingFileUrl = r.RecordingFileUrl,
                CallSummary = r.CallSummary
            })
            .ToArrayAsync();

        return new WorkflowStageDetailsDto
        {
            CaseId = caseItem.Id,
            CaseNumber = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString(),
            CurrentStage = caseItem.CurrentStage,
            Status = caseItem.Status.ToString(),
            Admissibility = caseItem.AdmissibilityAssessment == null ? null : new AdmissibilityAssessmentDto
            {
                IsNotAnonymous = caseItem.AdmissibilityAssessment.IsNotAnonymous,
                IsNotInCourt = caseItem.AdmissibilityAssessment.IsNotInCourt,
                IsWithinMandate = caseItem.AdmissibilityAssessment.IsWithinMandate,
                HasSupportingDocuments = caseItem.AdmissibilityAssessment.HasSupportingDocuments,
                HasExhaustedInternalProcedures = caseItem.AdmissibilityAssessment.HasExhaustedInternalProcedures,
                IsAdmissible = caseItem.AdmissibilityAssessment.IsAdmissible,
                ScreeningNotes = caseItem.AdmissibilityAssessment.ScreeningNotes,
                RejectionReason = caseItem.AdmissibilityAssessment.RejectionReason
            },
            MediationSessions = caseItem.MediationLogs.Select(m => new MediationLogDto
            {
                SessionDate = m.SessionDate,
                Attendees = m.Attendees,
                SummaryOfDiscussions = m.SummaryOfDiscussions,
                SettlementProposal = m.SettlementProposal,
                IsAmicablySettled = m.IsAmicablySettled,
                AgreementDocumentUrl = m.AgreementDocumentUrl
            }).ToArray(),
            QaReviews = caseItem.QualityAssuranceReviews.Select(q => new QualityAssuranceReviewDto
            {
                AccuracyVerified = q.AccuracyVerified,
                ConsistencyVerified = q.ConsistencyVerified,
                LegalComplianceVerified = q.LegalComplianceVerified,
                PolicyAdherenceVerified = q.PolicyAdherenceVerified,
                IsApprovedForDecision = q.IsApprovedForDecision,
                QaComments = q.QaComments,
                RevisionInstructions = q.RevisionInstructions
            }).ToArray(),
            Decision = caseItem.Decision == null ? null : new CaseDecisionDto
            {
                DecisionSummary = caseItem.Decision.DecisionSummary,
                LegalBasisCitations = caseItem.Decision.LegalBasisCitations,
                RecommendationsApproved = caseItem.Decision.RecommendationsApproved,
                DecisionDocumentUrl = caseItem.Decision.DecisionDocumentUrl,
                IssuerTitle = caseItem.Decision.IssuerTitle
            },
            CallRecords = callRecords
        };
    }
}
