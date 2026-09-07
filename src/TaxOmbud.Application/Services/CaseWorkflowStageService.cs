using TaxOmbud.Application.Cases.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Constants;
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
        caseItem = new Case(complaint.Id, complaint.Subject, account.Id, complaint.Priority.ToString(), complaint.IntakeChannel);
        caseItem.Open(ReferenceNumber.From(caseNumberStr));
        caseItem.UpdateStatus(CaseStatus.Submitted, WorkflowStage.Intake, userId);

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

        var existingCase = await EnsureCaseExistsAsync(complaintId, registeredBy);
        if (existingCase != null)
        {
            existingCase.UpdateStatus(CaseStatus.Registered, WorkflowStage.RegistrationAndAcknowledgement, registeredBy);
        }

        await _context.SaveChangesAsync();

        var complainantEmail = complaint.Taxpayer?.User?.Email;
        var complainantName = complaint.Taxpayer?.User != null
            ? $"{complaint.Taxpayer.User.FirstName} {complaint.Taxpayer.User.LastName}"
            : "Complainant";

        await SendStageNotificationWithAuditCopyAsync(
            complainantEmail ?? string.Empty,
            complainantName,
            "Your Complaint Has Been Formally Registered",
            $"<p>Your complaint has been formally registered with the Office of the Tax Ombud. A <strong>Case Reference Number</strong> has been assigned and an investigating officer will be allocated shortly.</p>",
            registeredBy,
            "Stage 2 — Registration & Acknowledgement",
            complaint.ReferenceNumber);

        return true;
    }

    /// <summary>
    /// Stage 3 — Initial Review & Assignment.
    /// CE performs initial review and optionally assigns to an officer and department.
    /// </summary>
    public async Task<bool> StartInitialReviewAsync(StartInitialReviewCommand cmd, Guid initiatedBy)
    {
        var caseItem = await EnsureCaseExistsAsync(cmd.CaseId, initiatedBy);
        if (caseItem == null) return false;

        caseItem.StartInitialReview();

        if (cmd.AssignedOfficerId.HasValue)
            caseItem.Assign(cmd.AssignedOfficerId.Value, initiatedBy);

        if (cmd.DepartmentId.HasValue)
            caseItem.DepartmentId = cmd.DepartmentId;

        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);
        complaint?.UpdateStatus(CaseStatus.UnderAssessment, WorkflowStage.InitialReviewAndAssignment);

        await _context.SaveChangesAsync();

        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        if (cmd.AssignedOfficerId.HasValue)
        {
            var officer = await _context.Users.FirstOrDefaultAsync(u => u.Id == cmd.AssignedOfficerId.Value);
            if (officer != null && !string.IsNullOrWhiteSpace(officer.Email))
            {
                await SendStageNotificationWithAuditCopyAsync(
                    officer.Email,
                    $"{officer.FirstName} {officer.LastName}",
                    "Case Assigned to You — Initial Review & Assignment",
                    $"<p>The Chief Executive has completed the Initial Review for Case <strong>{caseRef}</strong> and assigned it to you for Jurisdiction & Admissibility Assessment.</p>",
                    initiatedBy,
                    "Stage 3 — Initial Review & Assignment",
                    caseRef);
            }
        }

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
        assessment.JurisdictionCheck = dto.JurisdictionCheck;
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
            // ADMISSIBLE — advance to Stage 5: Investigation & Resolution
            caseItem.StartInvestigation();
            complainant?.UpdateStatus(CaseStatus.UnderInvestigation, WorkflowStage.InvestigationAndResolution);
        }
        else
        {
            // NOT ADMISSIBLE — formal declaration handled by DeclareNotAdmissibleAsync
            // This assessment save alone does NOT close the case; caller must invoke DeclareNotAdmissibleAsync
            caseItem.UpdateStatus(CaseStatus.Assigned, WorkflowStage.JurisdictionAndAdmissibility, assessedBy);
        }

        await _context.SaveChangesAsync();

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null
            ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}"
            : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        if (dto.IsAdmissible)
        {
            await SendStageNotificationWithAuditCopyAsync(
                cEmail ?? string.Empty,
                cName,
                "Your Case Has Been Declared ADMISSIBLE",
                "<p>Your case has passed the Jurisdiction & Admissibility Assessment and has been declared <strong style=\"color:#114a31;\">ADMISSIBLE</strong>. It is now advancing to the Investigation & Resolution stage.</p>",
                assessedBy,
                "Stage 4 — Jurisdiction & Admissibility Assessment",
                caseRef);
        }

        return true;
    }

    /// <summary>
    /// Stage 4 Terminal — Formally declares a complaint Not Admissible.
    /// Closes the case, creates a NotAdmissibleDecision record, and dispatches a formal notification to the taxpayer.
    /// </summary>
    public async Task<bool> DeclareNotAdmissibleAsync(DeclareNotAdmissibleCommand cmd, Guid declaredBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.Complaint).ThenInclude(cp => cp.Taxpayer).ThenInclude(tp => tp.User)
            .Include(c => c.NotAdmissibleDecision)
            .FirstOrDefaultAsync(c => c.Id == cmd.CaseId || c.ComplaintId == cmd.CaseId);

        if (caseItem == null) return false;
        if (caseItem.NotAdmissibleDecision != null) return false; // Already declared

        // Create formal record
        var decision = new NotAdmissibleDecision
        {
            Id = Guid.NewGuid(),
            CaseId = caseItem.Id,
            Reason = cmd.Reason,
            AdmissibilityAssessmentId = cmd.AdmissibilityAssessmentId,
            DeclaredByUserId = declaredBy,
            DeclaredAt = DateTimeOffset.UtcNow
        };
        _context.NotAdmissibleDecisions.Add(decision);

        // Close the case
        caseItem.Close("Not Admissible", cmd.Reason, declaredBy);

        // Close the complaint with NotAdmissible status
        var complaint = caseItem.Complaint;
        complaint?.DeclareNotAdmissible(cmd.Reason, declaredBy);

        // Cancel active workflow instance
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
                    t.Comment = "Cancelled: Complaint declared Not Admissible.";
                    t.PerformedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();

        // Dispatch formal Not Admissible notification to taxpayer
        var taxpayerEmail = complaint?.Taxpayer?.User?.Email;
        var taxpayerName = complaint?.Taxpayer?.User != null
            ? $"{complaint.Taxpayer.User.FirstName} {complaint.Taxpayer.User.LastName}"
            : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            taxpayerEmail ?? string.Empty,
            taxpayerName,
            $"Formal Determination: Complaint {caseRef} — NOT ADMISSIBLE",
            $"""
            <p>Following a thorough Jurisdiction and Admissibility Assessment, the Office of the Tax Ombud has formally determined that your complaint is <strong style="color:#c0392b;">NOT ADMISSIBLE</strong> for investigation.</p>
            <div style="background:#fef2f2;border-left:4px solid #dc2626;padding:14px 18px;margin:16px 0;border-radius:4px;">
              <strong style="color:#991b1b;display:block;margin-bottom:4px;">Reason:</strong>
              <span style="color:#1f2937;">{cmd.Reason}</span>
            </div>
            <p style="font-size:.85rem;color:#4b5563;">If you believe this determination was made in error, or if you have new information not previously considered, you may contact the Office of the Tax Ombud to seek further guidance.</p>
            """,
            declaredBy,
            "Stage 4 — Not Admissible (Terminal)",
            caseRef);

        // Mark notification sent
        decision.NotificationSent = true;
        decision.NotificationSentAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

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
                "Case Assigned to You — Jurisdiction & Admissibility Assessment",
                $"<p>You have been assigned as the Case Officer for case <strong>{caseRef}</strong>. Please proceed with the Jurisdiction & Admissibility Assessment.</p>",
                assignedBy,
                "Stage 3 — Initial Review & Assignment",
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

        // Ensure case is in Investigation stage (sub-activity within Stage 5)
        if (caseItem.Status != CaseStatus.UnderInvestigation)
            caseItem.StartInvestigation();

        await _context.SaveChangesAsync();

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null
            ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}"
            : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            "Mediation Session Logged — Investigation & Resolution",
            $"<p>A dispute resolution / mediation session has been recorded for your case. Status: {(dto.IsAmicablySettled ? "Amicably Settled" : "Ongoing Resolution")}.</p>",
            loggedBy,
            "Stage 5 — Investigation & Resolution",
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

        // QA is a sub-activity within Stage 5 — stage slug remains 5_investigation
        if (dto.IsApprovedForDecision)
            caseItem.UpdateStatus(CaseStatus.UnderInvestigation, WorkflowStage.InvestigationAndResolution, reviewedBy);

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
                $"Supervisory QA Review — Stage 5: {qaStatus}",
                $"<p>Supervisory Quality Assurance review for case <strong>{caseRef}</strong> has been marked as <strong>{qaStatus}</strong>.</p>",
                reviewedBy,
                "Stage 5 — Investigation & Resolution (QA Review)",
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

        // Mirror to complaint
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);
        complaint?.UpdateStatus(CaseStatus.DecisionIssued, WorkflowStage.DecisionAndCommunication);

        await _context.SaveChangesAsync();

        var complainant = await _context.Complaints
            .Include(c => c.Taxpayer).ThenInclude(tp => tp.User)
            .FirstOrDefaultAsync(c => c.Id == caseItem.ComplaintId);

        var cEmail = complainant?.Taxpayer?.User?.Email;
        var cName = complainant?.Taxpayer?.User != null
            ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}"
            : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        var decisionLetterButton = !string.IsNullOrWhiteSpace(dto.DecisionDocumentUrl)
            ? $"<p style=\"margin-top:16px;\"><a href=\"{dto.DecisionDocumentUrl}\" style=\"background:#114a31;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;font-weight:bold;\">Download Decision Letter</a></p>"
            : string.Empty;

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            $"Formal Decision Issued — Case {caseRef}",
            $"<p>The Chief Executive of the Office of the Tax Ombud has formally issued the Decision & Communication for your case.</p><p><strong>Decision Summary:</strong> {dto.DecisionSummary}</p>{decisionLetterButton}",
            issuedBy,
            "Stage 6 — Decision & Communication",
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

        // Cancel active workflow instance
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
        var cName = complainant?.Taxpayer?.User != null
            ? $"{complainant.Taxpayer.User.FirstName} {complainant.Taxpayer.User.LastName}"
            : "Complainant";
        var caseRef = caseItem.CaseNumber?.Value ?? caseItem.Id.ToString();

        await SendStageNotificationWithAuditCopyAsync(
            cEmail ?? string.Empty,
            cName,
            $"Case {caseRef} — Formally Closed & Archived",
            $"<p>Your case <strong>{caseRef}</strong> has been formally closed and securely archived by the Office of the Tax Ombud.</p><p><strong>Outcome:</strong> {outcome}</p>",
            closedBy,
            "Stage 7 — Closure & Archiving",
            caseRef);

        return true;
    }

    /// <summary>
    /// Stage 7 — Formal Archiving.
    /// Creates a CaseArchiveRecord and marks the case as archived.
    /// </summary>
    public async Task<bool> ArchiveCaseAsync(ArchiveCaseCommand cmd, Guid archivedBy)
    {
        var caseItem = await _context.Cases
            .Include(c => c.ArchiveRecord)
            .FirstOrDefaultAsync(c => c.Id == cmd.CaseId || c.ComplaintId == cmd.CaseId);

        if (caseItem == null) return false;
        if (caseItem.ArchiveRecord != null) return false; // Already archived

        var archiveRecord = new CaseArchiveRecord
        {
            Id = Guid.NewGuid(),
            CaseId = caseItem.Id,
            ArchivePurpose = cmd.ArchivePurpose,
            ArchivedDocumentRefs = cmd.ArchivedDocumentRefs,
            ArchivedByUserId = archivedBy,
            ArchivedAt = DateTimeOffset.UtcNow
        };

        _context.CaseArchiveRecords.Add(archiveRecord);
        caseItem.Archive();

        await _context.SaveChangesAsync();
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
        // Load case with all sub-stage data for details view
        var caseItem = await _context.Cases
            .Include(c => c.AdmissibilityAssessment)
            .Include(c => c.MediationLogs)
            .Include(c => c.Findings)
            .Include(c => c.Recommendations)
            .Include(c => c.QualityAssuranceReviews)
            .Include(c => c.Decision)
            .FirstOrDefaultAsync(c => c.Id == caseId || c.ComplaintId == caseId);
        // (EnsureCaseExistsAsync used for other methods; direct query used here for includes)

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
                JurisdictionCheck = caseItem.AdmissibilityAssessment.JurisdictionCheck,
                IsAdmissible = caseItem.AdmissibilityAssessment.IsAdmissible,
                ScreeningNotes = caseItem.AdmissibilityAssessment.ScreeningNotes,
                RejectionReason = caseItem.AdmissibilityAssessment.RejectionReason
            },
            Findings = caseItem.Findings.Select(f => new CaseFindingDto(f.Id, f.CaseId, f.Description, DateTimeOffset.Parse(f.CreatedAt.ToString("o")), f.CreatedByUserId)).ToArray(),
            Recommendations = caseItem.Recommendations.Select(r => new CaseRecommendationDto
            {
                Id = r.Id,
                RecommendationText = r.RecommendationText,
                Status = r.Status,
                Notes = r.Notes
            }).ToArray(),
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
                    Guid.Empty,
                    "Stage 7 — Closure & Archiving",
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
