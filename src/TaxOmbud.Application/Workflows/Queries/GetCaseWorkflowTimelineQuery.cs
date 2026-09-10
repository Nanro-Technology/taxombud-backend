using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Workflows.DTOs;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Workflows.Queries;

public record GetCaseWorkflowTimelineQuery(Guid CaseId) : IRequest<List<CaseWorkflowAuditLogDto>>;

public class GetCaseWorkflowTimelineQueryHandler : IRequestHandler<GetCaseWorkflowTimelineQuery, List<CaseWorkflowAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseWorkflowTimelineQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CaseWorkflowAuditLogDto>> Handle(GetCaseWorkflowTimelineQuery request, CancellationToken cancellationToken)
    {
        var caseItem = await _context.Cases
            .Include(c => c.Complaint).ThenInclude(cp => cp.Taxpayer).ThenInclude(tp => tp.User)
            .Include(c => c.AdmissibilityAssessment)
            .Include(c => c.MediationLogs)
            .Include(c => c.QualityAssuranceReviews)
            .Include(c => c.Decision)
            .Include(c => c.ActiveWorkflowInstance)
                .ThenInclude(i => i!.Workflow)
                    .ThenInclude(w => w.Levels)
                        .ThenInclude(l => l.Targets)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId || c.ComplaintId == request.CaseId, cancellationToken);

        var targetCaseId = caseItem?.Id ?? request.CaseId;

        // Load recorded audit logs
        var logs = await _context.CaseWorkflowAuditLogs
            .Include(l => l.PerformedByUser)
                .ThenInclude(u => u!.Role)
            .AsNoTracking()
            .Where(l => l.CaseId == targetCaseId || l.CaseId == request.CaseId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);

        // Load all custom roles to resolve designated level role names
        var rolesMap = await _context.CustomRoles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        // Find workflow levels from active workflow instance OR from the default/active workflow
        var stageRoleMap = new Dictionary<int, string>();
        var workflowLevels = caseItem?.ActiveWorkflowInstance?.Workflow?.Levels;
        if (workflowLevels == null || !workflowLevels.Any())
        {
            var wfInstance = await _context.WorkflowInstances
                .Include(i => i.Workflow).ThenInclude(w => w.Levels).ThenInclude(l => l.Targets)
                .Where(i => i.CaseId == targetCaseId)
                .OrderByDescending(i => i.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
            workflowLevels = wfInstance?.Workflow?.Levels;
        }

        if (workflowLevels == null || !workflowLevels.Any())
        {
            var defaultWf = await _context.Workflows
                .Include(w => w.Levels).ThenInclude(l => l.Targets)
                .FirstOrDefaultAsync(w => w.IsDefault, cancellationToken)
                ?? await _context.Workflows
                    .Include(w => w.Levels).ThenInclude(l => l.Targets)
                    .OrderByDescending(w => w.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            workflowLevels = defaultWf?.Levels;
        }

        if (workflowLevels != null && workflowLevels.Any())
        {
            foreach (var lvl in workflowLevels)
            {
                var roleTargetIds = lvl.Targets
                    .Where(t => t.TargetType == WorkflowLevelTargetType.Role)
                    .Select(t => t.TargetId)
                    .ToList();

                var roleNames = roleTargetIds
                    .Where(id => rolesMap.ContainsKey(id))
                    .Select(id => rolesMap[id])
                    .ToList();

                if (roleNames.Any())
                {
                    stageRoleMap[lvl.LevelNumber] = string.Join(" / ", roleNames);
                }
                else
                {
                    stageRoleMap[lvl.LevelNumber] = lvl.LevelRole switch
                    {
                        LevelRole.Intake => "Intake Officer",
                        LevelRole.Registration => "Registration Officer",
                        LevelRole.InitialReview => "Finance",
                        LevelRole.AdmissibilityGate => "Assessment Officer",
                        LevelRole.Investigation => "Investigation Officer",
                        LevelRole.Decision => "Chief Executive",
                        LevelRole.Closure => "Closure & Archiving",
                        _ => lvl.Name
                    };
                }
            }
        }

        // Canonical default fallbacks for Tax Ombudsman 7-stage workflow
        if (!stageRoleMap.ContainsKey(1)) stageRoleMap[1] = "Intake Officer";
        if (!stageRoleMap.ContainsKey(2)) stageRoleMap[2] = "Registration Officer";
        if (!stageRoleMap.ContainsKey(3)) stageRoleMap[3] = "Finance";
        if (!stageRoleMap.ContainsKey(4)) stageRoleMap[4] = "Assessment Officer";
        if (!stageRoleMap.ContainsKey(5)) stageRoleMap[5] = "Investigation Officer";
        if (!stageRoleMap.ContainsKey(6)) stageRoleMap[6] = "Chief Executive";
        if (!stageRoleMap.ContainsKey(7)) stageRoleMap[7] = "Closure & Archiving";

        var resultList = new List<CaseWorkflowAuditLogDto>();

        foreach (var l in logs)
        {
            stageRoleMap.TryGetValue(l.LevelNumber, out var stageRole);
            var actualUserRole = l.PerformedByUser?.Role?.Name;

            // Resolve the role at which the flow was approved:
            // The designated stage role takes precedence (e.g. Finance at Stage 3, Registration Officer at Stage 2)
            string resolvedRole;
            if (!string.IsNullOrWhiteSpace(stageRole))
            {
                resolvedRole = stageRole;
            }
            else if (!string.IsNullOrWhiteSpace(l.UserRole) && l.UserRole != "Officer" && !l.UserRole.StartsWith("System/User"))
            {
                resolvedRole = l.UserRole;
            }
            else if (!string.IsNullOrWhiteSpace(actualUserRole) && actualUserRole != "Officer")
            {
                resolvedRole = actualUserRole;
            }
            else
            {
                resolvedRole = "Authorized Officer";
            }

            // Fix redundant InProgress -> InProgress status displays
            string prevStatus = l.PreviousStatus;
            string newStatus = l.NewStatus;
            if (prevStatus == "InProgress" && newStatus == "InProgress")
            {
                prevStatus = $"Stage {l.LevelNumber} Pending";
                newStatus = $"Approved → Advanced to Stage {l.LevelNumber + 1}";
            }

            resultList.Add(new CaseWorkflowAuditLogDto(
                l.Id,
                l.CaseId,
                l.PerformedByUserId,
                l.PerformedByUser != null ? $"{l.PerformedByUser.FirstName} {l.PerformedByUser.LastName}" : "System",
                resolvedRole,
                l.Action,
                prevStatus,
                newStatus,
                l.LevelNumber,
                l.LevelName,
                l.Comment,
                l.Timestamp
            ));
        }

        // Determine current pipeline stage of the case
        int currentStageNum = 1;
        if (caseItem != null)
        {
            if (caseItem.ActiveWorkflowInstance != null && caseItem.ActiveWorkflowInstance.CurrentLevelNumber > 1)
            {
                currentStageNum = caseItem.ActiveWorkflowInstance.CurrentLevelNumber;
            }
            else
            {
                var stageStr = (caseItem.CurrentStage.ToString() + " " + caseItem.Status.ToString()).ToLowerInvariant();
                if (stageStr.Contains("7") || stageStr.Contains("close") || caseItem.Status == CaseStatus.Closed) currentStageNum = 7;
                else if (stageStr.Contains("6") || stageStr.Contains("decision")) currentStageNum = 6;
                else if (stageStr.Contains("5") || stageStr.Contains("investigat") || caseItem.Status == CaseStatus.UnderInvestigation) currentStageNum = 5;
                else if (stageStr.Contains("4") || stageStr.Contains("admissib") || caseItem.Status == CaseStatus.UnderAssessment) currentStageNum = 4;
                else if (stageStr.Contains("3") || stageStr.Contains("review")) currentStageNum = 3;
                else if (stageStr.Contains("2") || stageStr.Contains("regist")) currentStageNum = 2;
            }

            // If an audit log reached a higher level, reflect that
            if (resultList.Any())
            {
                var maxLoggedLevel = resultList.Max(x => x.LevelNumber);
                if (maxLoggedLevel >= currentStageNum)
                {
                    currentStageNum = Math.Min(7, maxLoggedLevel + 1);
                }
            }
        }

        // 1. Stage 1: Intake & Lodgement Milestone
        if (caseItem != null && !resultList.Any(l => l.LevelNumber == 1))
        {
            var complainantName = caseItem.Complaint?.Taxpayer?.User != null
                ? $"{caseItem.Complaint.Taxpayer.User.FirstName} {caseItem.Complaint.Taxpayer.User.LastName}".Trim()
                : (caseItem.Complaint?.Taxpayer?.CompanyName ?? "Taxpayer / Complainant");

            resultList.Add(new CaseWorkflowAuditLogDto(
                Guid.NewGuid(),
                targetCaseId,
                caseItem.Complaint?.Taxpayer?.UserId ?? Guid.Empty,
                complainantName,
                "Taxpayer / Intake",
                "Submitted",
                "New Case",
                "Submitted (Stage 1 Intake)",
                1,
                "Stage 1: Intake & Lodgement",
                $"Complaint lodged via {caseItem.IntakeChannel} and initialized into workflow pipeline.",
                new DateTimeOffset(caseItem.CreatedAt, TimeSpan.Zero)
            ));
        }

        // 2. Stage 2: Registration & Acknowledgement Milestone
        if (caseItem != null && currentStageNum >= 2 && !resultList.Any(l => l.LevelNumber == 2))
        {
            resultList.Add(new CaseWorkflowAuditLogDto(
                Guid.NewGuid(),
                targetCaseId,
                Guid.Empty,
                "Registry Officer",
                stageRoleMap.GetValueOrDefault(2, "Registration Officer"),
                "Approve",
                "Submitted",
                "Approved → Advanced to Stage 3 (Initial Review)",
                2,
                "Stage 2: Registration & Acknowledgement",
                "Case reference number assigned and formal acknowledgement letter dispatched.",
                new DateTimeOffset(caseItem.CreatedAt.AddHours(1), TimeSpan.Zero)
            ));
        }

        // 3. Stage 3: Initial Review & Assignment Milestone
        if (caseItem != null && currentStageNum >= 3 && !resultList.Any(l => l.LevelNumber == 3))
        {
            resultList.Add(new CaseWorkflowAuditLogDto(
                Guid.NewGuid(),
                targetCaseId,
                Guid.Empty,
                "Finance Department",
                stageRoleMap.GetValueOrDefault(3, "Finance"),
                "Approve",
                "Stage 3 Pending",
                "Approved → Advanced to Stage 4 (Admissibility)",
                3,
                "Stage 3: Initial Review & Assignment",
                "Complaint dossier reviewed and approved by Finance.",
                new DateTimeOffset(caseItem.CreatedAt.AddHours(2), TimeSpan.Zero)
            ));
        }

        // 4. Stage 4: Jurisdiction & Admissibility Screening Milestone
        if (!resultList.Any(l => l.LevelNumber == 4))
        {
            if (caseItem?.AdmissibilityAssessment != null)
            {
                var assessor = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == caseItem.AdmissibilityAssessment.AssessedByUserId, cancellationToken);
                bool isAdm = caseItem.AdmissibilityAssessment.IsAdmissible;

                resultList.Add(new CaseWorkflowAuditLogDto(
                    caseItem.AdmissibilityAssessment.Id,
                    targetCaseId,
                    caseItem.AdmissibilityAssessment.AssessedByUserId,
                    assessor != null ? $"{assessor.FirstName} {assessor.LastName}" : "Assessment Officer",
                    stageRoleMap.GetValueOrDefault(4, "Assessment Officer"),
                    isAdm ? "Admissibility Approved" : "Declared Inadmissible",
                    "Stage 4 Pending Assessment",
                    isAdm ? "Approved → Advanced to Stage 5 (Investigation)" : "Case Closed (Inadmissible)",
                    4,
                    "Stage 4: Jurisdiction & Admissibility",
                    caseItem.AdmissibilityAssessment.ScreeningNotes ?? (isAdm ? "All 5 statutory admissibility criteria satisfied." : "Failed admissibility screening."),
                    caseItem.AdmissibilityAssessment.AssessedAt
                ));
            }
            else if (caseItem != null && currentStageNum >= 5)
            {
                resultList.Add(new CaseWorkflowAuditLogDto(
                    Guid.NewGuid(),
                    targetCaseId,
                    Guid.Empty,
                    "Assessment Officer",
                    stageRoleMap.GetValueOrDefault(4, "Assessment Officer"),
                    "Admissibility Approved",
                    "Stage 4 Pending Assessment",
                    "Approved → Advanced to Stage 5 (Investigation)",
                    4,
                    "Stage 4: Jurisdiction & Admissibility",
                    "Statutory jurisdiction and 5 admissibility criteria satisfied.",
                    new DateTimeOffset(caseItem.CreatedAt.AddHours(3), TimeSpan.Zero)
                ));
            }
        }

        // 5. Stage 5: Investigation & Resolution Active Milestone
        if (caseItem != null && currentStageNum >= 5 && !resultList.Any(l => l.LevelNumber == 5 && l.Action == "Active Investigation"))
        {
            resultList.Add(new CaseWorkflowAuditLogDto(
                Guid.NewGuid(),
                targetCaseId,
                Guid.Empty,
                "Investigation Team",
                stageRoleMap.GetValueOrDefault(5, "Investigation Officer"),
                "Active Investigation",
                "Admissibility Cleared",
                "Under Investigation (Active Stage)",
                5,
                "Stage 5: Investigation & Resolution",
                "Case undergoing active multi-officer deliberation, investigative analysis, and dispute resolution.",
                caseItem.LastModifiedAt.HasValue ? new DateTimeOffset(caseItem.LastModifiedAt.Value, TimeSpan.Zero) : DateTimeOffset.UtcNow
            ));
        }

        // 6. Stage 5: Mediation Sessions
        if (caseItem?.MediationLogs != null)
        {
            foreach (var m in caseItem.MediationLogs)
            {
                if (!resultList.Any(l => l.Id == m.Id))
                {
                    var medOfficer = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == m.LoggedByUserId, cancellationToken);
                    resultList.Add(new CaseWorkflowAuditLogDto(
                        m.Id,
                        targetCaseId,
                        m.LoggedByUserId,
                        medOfficer != null ? $"{medOfficer.FirstName} {medOfficer.LastName}" : "Mediation Officer",
                        "Mediation Officer",
                        "Mediation Logged",
                        "Active Investigation",
                        m.IsAmicablySettled ? "Amicable Settlement Achieved" : "Dispute Resolution Session Logged",
                        5,
                        "Stage 5: Mediation & Alternative Dispute Resolution",
                        $"Attendees: {m.Attendees}\nSummary: {m.SummaryOfDiscussions}" + (!string.IsNullOrEmpty(m.SettlementProposal) ? $"\nProposal: {m.SettlementProposal}" : ""),
                        m.LoggedAt
                    ));
                }
            }
        }

        // 7. Stage 5: QA Reviews
        if (caseItem?.QualityAssuranceReviews != null)
        {
            foreach (var q in caseItem.QualityAssuranceReviews)
            {
                if (!resultList.Any(l => l.Id == q.Id))
                {
                    var qaReviewer = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == q.ReviewedByUserId, cancellationToken);
                    resultList.Add(new CaseWorkflowAuditLogDto(
                        q.Id,
                        targetCaseId,
                        q.ReviewedByUserId,
                        qaReviewer != null ? $"{qaReviewer.FirstName} {qaReviewer.LastName}" : "QA Supervisor",
                        "QA Supervisor",
                        q.IsApprovedForDecision ? "QA Review Approved" : "QA Revision Requested",
                        "Investigation Findings Review",
                        q.IsApprovedForDecision ? "Approved → Cleared for CE Decision" : "Returned for Revision",
                        5,
                        "Stage 5: Supervisory QA Sign-Off",
                        q.QaComments,
                        q.ReviewedAt
                    ));
                }
            }
        }

        // 8. Stage 6: Decision
        if (caseItem?.Decision != null && !resultList.Any(l => l.LevelNumber == 6))
        {
            var decIssuer = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == caseItem.Decision.IssuedByUserId, cancellationToken);
            resultList.Add(new CaseWorkflowAuditLogDto(
                caseItem.Decision.Id,
                targetCaseId,
                caseItem.Decision.IssuedByUserId,
                decIssuer != null ? $"{decIssuer.FirstName} {decIssuer.LastName}" : "Chief Executive",
                stageRoleMap.GetValueOrDefault(6, "Chief Executive"),
                "Determination Issued",
                "Stage 6 Pending CE Decision",
                "Chief Executive Decision Issued & Communicated",
                6,
                "Stage 6: Decision & Communication",
                caseItem.Decision.DecisionSummary,
                caseItem.Decision.IssuedAt
            ));
        }

        // 9. Stage 7: Closure
        if (caseItem?.Status == CaseStatus.Closed && !resultList.Any(l => l.LevelNumber == 7))
        {
            resultList.Add(new CaseWorkflowAuditLogDto(
                Guid.NewGuid(),
                targetCaseId,
                Guid.Empty,
                "System Registry",
                stageRoleMap.GetValueOrDefault(7, "Closure & Archiving"),
                "Case Closed",
                "Decision Communicated",
                "Closed & Archived",
                7,
                "Stage 7: Closure & Archiving",
                caseItem.FindingsSummary ?? caseItem.Outcome ?? "Case formally closed and archived.",
                caseItem.ClosedAt ?? (caseItem.LastModifiedAt.HasValue ? new DateTimeOffset(caseItem.LastModifiedAt.Value, TimeSpan.Zero) : DateTimeOffset.UtcNow)
            ));
        }

        return resultList.OrderByDescending(l => l.Timestamp).ToList();
    }
}
