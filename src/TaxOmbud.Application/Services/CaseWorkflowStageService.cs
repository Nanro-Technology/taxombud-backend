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

using TaxOmbud.Application.Interfaces.InfrastructureService;
using Microsoft.Extensions.Logging;

namespace TaxOmbud.Application.Services;

public class CaseWorkflowStageService : ICaseWorkflowStageService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<CaseWorkflowStageService> _logger;

    public CaseWorkflowStageService(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<CaseWorkflowStageService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    private async Task SendStageNotificationWithAuditCopyAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string bodyContent,
        Guid initiatorUserId,
        string stageName,
        string caseRef)
    {
        try
        {
            var formattedBody = $"""
                <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;">
                  <div style="background:#114a31;padding:24px 32px;text-align:center;border-bottom:4px solid #c9a227;">
                    <h1 style="color:#ffffff;font-size:1.1rem;margin:0;text-transform:uppercase;">OFFICE OF THE TAX OMBUD</h1>
                    <p style="color:rgba(255,255,255,.75);font-size:.8rem;margin:4px 0 0;">Federal Republic of Nigeria</p>
                  </div>
                  <div style="padding:28px 32px;background:#ffffff;color:#333333;font-size:.95rem;line-height:1.7;">
                    <h2 style="color:#114a31;font-size:1.15rem;margin-top:0;">{subject}</h2>
                    <p>Hello <strong>{recipientName}</strong>,</p>
                    {bodyContent}
                    <div style="background:#f8f9fa;border-left:4px solid #114a31;padding:12px 16px;margin:20px 0;font-size:.9rem;">
                      <p style="margin:0;"><strong>Case Reference:</strong> {caseRef}</p>
                      <p style="margin:4px 0 0;"><strong>Pipeline Stage:</strong> {stageName}</p>
                    </div>
                  </div>
                  <div style="background:#114a31;padding:16px 32px;text-align:center;">
                    <p style="color:#c9a227;font-size:.85rem;font-weight:bold;margin:0;">Office of the Tax Ombud</p>
                  </div>
                </div>
                """;

            if (!string.IsNullOrWhiteSpace(recipientEmail))
            {
                await _emailService.SendAsync(recipientEmail, subject, formattedBody);
            }

            // Dispatch audit status copy to Initiator
            var initiator = await _context.Users.FirstOrDefaultAsync(u => u.Id == initiatorUserId);
            if (initiator != null && !string.IsNullOrWhiteSpace(initiator.Email) && initiator.Email != recipientEmail)
            {
                var auditHtml = $"""
                    <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                      <h3 style="color:#114a31;margin-top:0;">Audit Copy: Workflow Stage Update</h3>
                      <p>Hello <strong>{initiator.FirstName} {initiator.LastName}</strong>,</p>
                      <p>You executed stage <strong>{stageName}</strong> for Case <strong>{caseRef}</strong>.</p>
                      <p><strong>Notification Status:</strong> Email notification dispatched to target recipient ({recipientEmail}).</p>
                    </div>
                    """;
                await _emailService.SendAsync(initiator.Email, $"[Audit Copy] Case {caseRef}: {stageName}", auditHtml);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send stage notification for case {CaseRef}", caseRef);
        }
    }

    public async Task<bool> RegisterComplaintAsync(Guid complaintId, Guid registeredBy)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == complaintId);
        if (complaint == null) return false;

        // Transition complaint & underlying case status to Registered
        var existingCase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
        if (existingCase != null)
        {
            existingCase.UpdateStatus(CaseStatus.Registered, "2_registration", registeredBy);
        }

        await _context.SaveChangesAsync();

        var complainantEmail = complaint.Taxpayer?.User?.Email;
        var complainantName = complaint.Taxpayer?.User != null ? $"{complaint.Taxpayer.User.FirstName} {complaint.Taxpayer.User.LastName}" : "Complainant";
        await SendStageNotificationWithAuditCopyAsync(
            complainantEmail ?? string.Empty,
            complainantName,
            "Complaint Formally Registered",
            $"<p>Your complaint has been formally registered with the Tax Ombud Office.</p>",
            registeredBy,
            "2_registration",
            complaint.ReferenceNumber);

        return true;
    }


    public async Task<bool> AssessAdmissibilityAsync(Guid caseId, AdmissibilityAssessmentDto dto, Guid assessedBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.AdmissibilityAssessment)
            .FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);

        if (caseItem == null) return false;

        if (caseItem.AdmissibilityAssessment == null)
        {
            caseItem.AdmissibilityAssessment = new AdmissibilityAssessment
            {
                Id = Guid.NewGuid(),
                CaseId = caseItem.Id
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

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}" : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        var admissibilityStatus = dto.IsAdmissible ? "ADMISSIBLE" : "INADMISSIBLE";
        var bodyMsg = dto.IsAdmissible
            ? $"<p>Your case has passed statutory admissibility screening (Status: <strong>ADMISSIBLE</strong>) and is advancing to assignment and investigation.</p>"
            : $"<p>Your case was evaluated as <strong>INADMISSIBLE</strong>. Reason: {dto.RejectionReason}</p>";

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            $"Case Admissibility Screened: {admissibilityStatus}",
            bodyMsg,
            assessedBy,
            "3_assessment",
            caseRef);

        return true;
    }

    public async Task<bool> AssignCaseByCeAsync(Guid caseId, Guid officerId, Guid departmentId, Guid assignedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);
        if (caseItem == null) return false;

        caseItem.DepartmentId = departmentId;
        caseItem.Assign(officerId, assignedBy);

        await _context.SaveChangesAsync();

        var assignedOfficer = await _context.Users.FirstOrDefaultAsync(u => u.Id == officerId);
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        if (assignedOfficer != null && !string.IsNullOrWhiteSpace(assignedOfficer.Email))
        {
            await SendStageNotificationWithAuditCopyAsync(
                assignedOfficer.Email,
                $"{assignedOfficer.FirstName} {assignedOfficer.LastName}",
                "Case Assigned to You",
                $"<p>You have been assigned as the Case Officer for case <strong>{caseRef}</strong> by the Chief Executive.</p>",
                assignedBy,
                "4_assignment",
                caseRef);
        }

        return true;
    }

    public async Task<bool> LogMediationSessionAsync(Guid caseId, MediationLogDto dto, Guid loggedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);
        if (caseItem == null) return false;

        var log = new MediationLog
        {
            Id = Guid.NewGuid(),
            CaseId = caseItem.Id,
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

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}" : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            "Mediation Session Logged",
            $"<p>A dispute resolution / mediation session has been recorded for your case. Status: {(dto.IsAmicablySettled ? "Amicably Settled" : "Ongoing Resolution")}.</p>",
            loggedBy,
            "6_mediation",
            caseRef);

        return true;
    }

    public async Task<bool> SubmitQaReviewAsync(Guid caseId, QualityAssuranceReviewDto dto, Guid reviewedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);
        if (caseItem == null) return false;

        var qa = new QualityAssuranceReview
        {
            Id = Guid.NewGuid(),
            CaseId = caseItem.Id,
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

        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();
        var assignedOfficer = caseItem.AssignedOfficerId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == caseItem.AssignedOfficerId.Value)
            : null;

        if (assignedOfficer != null && !string.IsNullOrWhiteSpace(assignedOfficer.Email))
        {
            var qaStatus = dto.IsApprovedForDecision ? "APPROVED" : "REVISION REQUIRED";
            await SendStageNotificationWithAuditCopyAsync(
                assignedOfficer.Email,
                $"{assignedOfficer.FirstName} {assignedOfficer.LastName}",
                $"Supervisory QA Review Gate: {qaStatus}",
                $"<p>Supervisory Quality Assurance review for case <strong>{caseRef}</strong> has been marked as <strong>{qaStatus}</strong>.</p>",
                reviewedBy,
                "8_qa_review",
                caseRef);
        }

        return true;
    }

    public async Task<bool> IssueCeDecisionAsync(Guid caseId, CaseDecisionDto dto, Guid issuedBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.Decision)
            .FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);

        if (caseItem == null) return false;

        var decision = new CaseDecision
        {
            Id = Guid.NewGuid(),
            CaseId = caseItem.Id,
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

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}" : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            "Chief Executive Determination Issued",
            $"<p>The Chief Executive of the Tax Ombud Office has formally issued the Final Determination Decision for case <strong>{caseRef}</strong>.</p><p><strong>Summary:</strong> {dto.DecisionSummary}</p>",
            issuedBy,
            "9_ce_decision",
            caseRef);

        return true;
    }

    public async Task<bool> CloseAndArchiveCaseAsync(Guid caseId, string outcome, string summary, Guid closedBy)
    {
        var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);
        if (caseItem == null) return false;

        caseItem.Close(outcome, summary, closedBy);

        await _context.SaveChangesAsync();

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}" : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            "Case Closed & Archived",
            $"<p>Your case <strong>{caseRef}</strong> has been closed and securely archived in the Tax Ombud Vault.</p><p><strong>Outcome:</strong> {outcome}</p>",
            closedBy,
            "10_closure",
            caseRef);

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
            .FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);

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
