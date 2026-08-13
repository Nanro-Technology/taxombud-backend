using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Utilities;
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

    private async Task<Case?> EnsureCaseExistsAsync(Guid caseIdOrComplaintId, Guid userId)
    {
        var caseItem = await _context.Cases
            .Include(c => c.AdmissibilityAssessment)
            .Include(c => c.Decision)
            .FirstOrDefaultAsync(c => c.Id == caseIdOrComplaintId || c.ComplaintId == caseIdOrComplaintId);

        if (caseItem != null) return caseItem;

        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == caseIdOrComplaintId);
        if (complaint == null) return null;

        var account = await _context.Accounts.FirstOrDefaultAsync();
        if (account == null)
        {
            account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Headquarters Zonal Office",
                Email = "info@mediate.com.ng",
                Country = "Nigeria",
                Status = "active",
                IsWorkflowLane = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
        }

        var caseNumberStr = $"CASE-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        caseItem = new Case(complaint.Id, complaint.Subject, account.Id, complaint.Priority.ToString());
        caseItem.Open(ReferenceNumber.From(caseNumberStr));
        caseItem.UpdateStatus(CaseStatus.Submitted, "1_intake", userId);

        _context.Cases.Add(caseItem);
        await _context.SaveChangesAsync();

        return caseItem;
    }

    public async Task<bool> RegisterComplaintAsync(Guid complaintId, Guid registeredBy)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == complaintId);
        if (complaint == null) return false;

        // Transition complaint & underlying case status to Registered
        var existingCase = await EnsureCaseExistsAsync(complaintId, registeredBy);
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
        var caseItem = await EnsureCaseExistsAsync(caseId, assessedBy);
        if (caseItem == null) return false;

        var assessment = caseItem.AdmissibilityAssessment;
        if (assessment == null)
        {
            assessment = new AdmissibilityAssessment
            {
                Id = Guid.NewGuid(),
                CaseId = caseItem.Id
            };
            _context.AdmissibilityAssessments.Add(assessment);
            caseItem.AdmissibilityAssessment = assessment;
        }

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

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        if (dto.IsAdmissible)
        {
            caseItem.UpdateStatus(CaseStatus.UnderAssessment, "3_assessment", assessedBy);
            complainant?.UpdateStatus(CaseStatus.UnderAssessment, "3_assessment");
        }
        else
        {
            caseItem.Close("Inadmissible - Rejected at Assessment", dto.RejectionReason ?? "Screening failed admissibility criteria.", assessedBy);
            complainant?.Close(dto.RejectionReason ?? "Inadmissible - Rejected at Assessment", assessedBy);

            // Cancel active workflow instance and skip pending approval tasks
            if (caseItem.ActiveWorkflowInstanceId.HasValue)
            {
                var instance = await _context.WorkflowInstances
                    .Include(i => i.ApprovalTasks)
                    .FirstOrDefaultAsync(i => i.Id == caseItem.ActiveWorkflowInstanceId.Value);

                if (instance != null)
                {
                    instance.Complete(WorkflowStatus.Rejected);
                    foreach (var t in instance.ApprovalTasks.Where(t => t.TaskStatus == WorkflowLevelStatus.Pending))
                    {
                        t.TaskStatus = WorkflowLevelStatus.Skipped;
                        t.Comment = "Cancelled: Case failed admissibility screening.";
                        t.PerformedAt = DateTimeOffset.UtcNow;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}" : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        var admissibilityStatus = dto.IsAdmissible ? "ADMISSIBLE" : "INADMISSIBLE";

        var reasonText = !string.IsNullOrWhiteSpace(dto.RejectionReason)
            ? dto.RejectionReason
            : !string.IsNullOrWhiteSpace(dto.ScreeningNotes)
                ? dto.ScreeningNotes
                : null;

        // Build list of specific failed criteria
        var failedCriteria = new List<string>();
        if (!dto.IsNotAnonymous) failedCriteria.Add("Complaint submitted anonymously");
        if (!dto.IsNotInCourt) failedCriteria.Add("Matter is currently before a court or tribunal");
        if (!dto.IsWithinMandate) failedCriteria.Add("Subject matter falls outside Tax Ombud statutory mandate");
        if (!dto.HasSupportingDocuments) failedCriteria.Add("Insufficient supporting documentation provided");
        if (!dto.HasExhaustedInternalProcedures) failedCriteria.Add("Internal tax authority dispute procedures have not been exhausted");

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            reasonText = failedCriteria.Any()
                ? string.Join("; ", failedCriteria)
                : "Did not meet statutory admissibility requirements.";
        }

        var failedCriteriaHtml = failedCriteria.Any()
            ? $"""
              <div style="margin-top:10px;font-size:.88rem;color:#7f1d1d;">
                <strong>Specific Unmet Criteria:</strong>
                <ul style="margin:4px 0 0 18px;padding:0;">
                  {string.Join("", failedCriteria.Select(c => $"<li>{c}</li>"))}
                </ul>
              </div>
              """
            : string.Empty;

        var bodyMsg = dto.IsAdmissible
            ? $"<p>Your case has passed statutory admissibility screening (Status: <strong style=\"color:#114a31;\">ADMISSIBLE</strong>) and is advancing to assignment and investigation.</p>"
            : $"""
              <p>Your case was evaluated as <strong style="color:#c0392b;">INADMISSIBLE</strong> and cannot be admitted for investigation at this time.</p>
              <div style="background:#fef2f2;border-left:4px solid #dc2626;padding:14px 18px;margin:16px 0;border-radius:4px;">
                <strong style="color:#991b1b;display:block;margin-bottom:4px;">Reason for Inadmissibility:</strong>
                <span style="color:#1f2937;">{reasonText}</span>
                {failedCriteriaHtml}
              </div>
              <p style="font-size:.85rem;color:#4b5563;">If you believe this determination was made in error or if you have new supporting evidence, you may contact the Office of the Tax Ombud for further guidance.</p>
              """;

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
        var caseItem = await EnsureCaseExistsAsync(caseId, assignedBy);
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
        var caseItem = await EnsureCaseExistsAsync(caseId, loggedBy);
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
        var caseItem = await EnsureCaseExistsAsync(caseId, reviewedBy);
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
        var caseItem = await EnsureCaseExistsAsync(caseId, issuedBy);

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
        var caseItem = await EnsureCaseExistsAsync(caseId, closedBy);
        if (caseItem == null) return false;

        caseItem.Close(outcome, summary, closedBy);

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        complainant?.Close(outcome, closedBy);

        // Cancel active workflow instance and skip pending approval tasks
        if (caseItem.ActiveWorkflowInstanceId.HasValue)
        {
            var instance = await _context.WorkflowInstances
                .Include(i => i.ApprovalTasks)
                .FirstOrDefaultAsync(i => i.Id == caseItem.ActiveWorkflowInstanceId.Value);

            if (instance != null)
            {
                instance.Complete(WorkflowStatus.Cancelled);
                foreach (var t in instance.ApprovalTasks.Where(t => t.TaskStatus == WorkflowLevelStatus.Pending))
                {
                    t.TaskStatus = WorkflowLevelStatus.Skipped;
                    t.Comment = "Cancelled: Case closed and archived.";
                    t.PerformedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();

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
        var complaintId = dto.ComplaintId;
        var complaintExists = await _context.Complaints.AnyAsync(c => c.Id == complaintId);
        if (!complaintExists)
        {
            var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == dto.ComplaintId || c.ComplaintId == dto.ComplaintId);
            if (caseItem != null)
            {
                complaintId = caseItem.ComplaintId;
            }
        }

        var record = new CallCenterRecord
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
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
        var caseItem = await EnsureCaseExistsAsync(caseId, Guid.Empty);

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

    // ─── Case Closure Notifications ────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task SendCaseClosureNotificationsAsync(
        Guid caseId,
        Guid workflowInstanceId,
        string outcome,
        string? finalComment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ── 1. Load the case with its linked complaint → taxpayer → user ──
            var caseItem = await _context.Cases
                .Include(c => c.Complaint)
                    .ThenInclude(cp => cp.Taxpayer)
                        .ThenInclude(tp => tp.User)
                .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

            if (caseItem == null)
            {
                _logger.LogWarning("SendCaseClosureNotificationsAsync: Case {CaseId} not found — skipping notifications.", caseId);
                return;
            }

            var caseRef    = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();
            var closedAt   = caseItem.ClosedAt?.ToString("dd MMM yyyy, HH:mm UTC") ?? DateTimeOffset.UtcNow.ToString("dd MMM yyyy, HH:mm UTC");
            var complaint  = caseItem.Complaint;
            var taxpayer   = complaint?.Taxpayer;
            var lodgerUser = taxpayer?.User;

            // ── 2. Determine if corporate (Option C: same email, different subject/body) ──
            bool isCorporate = taxpayer != null && taxpayer.TaxpayerType != TaxpayerType.Individual;
            string? companyName = isCorporate ? (taxpayer!.CompanyName ?? "Your Organisation") : null;

            // ── 3. Collect recipients — use a dict to deduplicate by email ──
            var recipients = new Dictionary<string, (string Name, string Role)>(StringComparer.OrdinalIgnoreCase);

            // Lodger
            if (lodgerUser != null && !string.IsNullOrWhiteSpace(lodgerUser.Email))
            {
                var lodgerName = $"{lodgerUser.FirstName} {lodgerUser.LastName}".Trim();
                recipients[lodgerUser.Email] = (lodgerName, "Complainant");
            }

            // ── 4. Load every officer who executed a task on this workflow instance ──
            var actingTasks = await _context.CaseApprovalTasks
                .Include(t => t.AssignedUser)
                .Where(t => t.WorkflowInstanceId == workflowInstanceId && t.PerformedAt != null)
                .ToListAsync(cancellationToken);

            foreach (var approvalTask in actingTasks)
            {
                var officer = approvalTask.AssignedUser;
                if (officer == null || string.IsNullOrWhiteSpace(officer.Email)) continue;
                if (!recipients.ContainsKey(officer.Email))
                {
                    var officerName = $"{officer.FirstName} {officer.LastName}".Trim();
                    recipients[officer.Email] = (officerName, "Officer");
                }
            }

            // ── 5. Send personalized email to each recipient ──
            foreach (var (email, (name, role)) in recipients)
            {
                // Option C: for corporate complaints, prefix subject with company name
                var subjectSuffix = (isCorporate && role == "Complainant")
                    ? $" — {companyName}"
                    : string.Empty;

                var subject = $"Case {caseRef} Has Been {outcome}{subjectSuffix} | Office of the Tax Ombud";

                var corporateNotice = (isCorporate && role == "Complainant")
                    ? $"""
                      <div style="background:#fff8e1;border-left:4px solid #c9a227;padding:10px 16px;margin:16px 0;font-size:.9rem;">
                        <strong>Organisation:</strong> {companyName}<br/>
                        This notification is issued on behalf of the above-mentioned organisation.
                      </div>
                      """
                    : string.Empty;

                var outcomeColor = outcome.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "#114a31" : "#c0392b";
                var roleLabel    = role == "Officer" ? "As an officer who acted on this case, please keep a copy for your records." : "You are receiving this because you filed the original complaint.";

                var bodyContent = $"""
                                        <p>We write to formally notify you that the case referenced below has been <strong style="color:{outcomeColor};">{outcome}</strong> and officially closed.</p>
                    {corporateNotice}
                    <table style="width:100%;border-collapse:collapse;font-size:.9rem;margin:16px 0;">
                      <tr style="background:#f4f6f8;">
                        <td style="padding:8px 12px;font-weight:600;width:40%;">Case Reference</td>
                        <td style="padding:8px 12px;">{caseRef}</td>
                      </tr>
                      <tr>
                        <td style="padding:8px 12px;font-weight:600;">Outcome</td>
                        <td style="padding:8px 12px;color:{outcomeColor};font-weight:bold;">{outcome}</td>
                      </tr>
                      <tr style="background:#f4f6f8;">
                        <td style="padding:8px 12px;font-weight:600;">Closed At</td>
                        <td style="padding:8px 12px;">{closedAt}</td>
                      </tr>
                      <tr>
                        <td style="padding:8px 12px;font-weight:600;">Subject</td>
                        <td style="padding:8px 12px;">{caseItem.Subject}</td>
                      </tr>
                      {(string.IsNullOrWhiteSpace(finalComment) ? "" : $"""
                      <tr style="background:#f4f6f8;">
                        <td style="padding:8px 12px;font-weight:600;">Final Remarks</td>
                        <td style="padding:8px 12px;">{finalComment}</td>
                      </tr>
                      """)}
                    </table>
                    <p style="font-size:.85rem;color:#666;">{roleLabel}</p>
                    <p>Should you have any enquiries regarding this outcome, please contact the Office of the Tax Ombud directly.</p>
                    """;

                await SendStageNotificationWithAuditCopyAsync(
                    email,
                    name,
                    subject,
                    bodyContent,
                    Guid.Empty,           // no initiator audit copy needed for closure broadcast
                    "10_closure",
                    caseRef);
            }

            _logger.LogInformation(
                "Case closure notifications dispatched for Case {CaseRef} (outcome: {Outcome}) to {Count} recipient(s).",
                caseRef, outcome, recipients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send case closure notifications for CaseId {CaseId}", caseId);
        }
    }
}
