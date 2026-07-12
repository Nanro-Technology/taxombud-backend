using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/hr/people-ops")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class PeopleOpsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PeopleOpsController(IApplicationDbContext context)
    {
        _context = context;
    }

    // ─── PERFORMANCE SETTINGS ──────────────────────────────────────────────────

    [HttpGet("performance/settings/competencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompetencies(CancellationToken ct)
    {
        var competencies = await _context.Competencies
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var list = competencies.Select(c => new {
            id = c.Id.ToString(),
            name = c.Name,
            description = c.Description,
            sortOrder = c.SortOrder,
            status = c.Status
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = list });
    }

    [HttpPost("performance/settings/competencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveCompetency([FromBody] CreateCompetencyRequest request, CancellationToken ct)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await _context.Competencies.FindAsync(new object[] { request.Id.Value }, ct);
            if (existing != null)
            {
                existing.Name = request.Name;
                existing.Description = request.Description;
                existing.SortOrder = request.SortOrder;
                existing.Status = request.Status;
                existing.LastModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = existing });
            }
        }

        var competency = new Competency
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            SortOrder = request.SortOrder,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Competencies.AddAsync(competency, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = competency });
    }

    [HttpDelete("performance/settings/competencies/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCompetency(Guid id, CancellationToken ct)
    {
        var existing = await _context.Competencies.FindAsync(new object[] { id }, ct);
        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = true });
    }

    [HttpGet("performance/settings/templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewTemplates(CancellationToken ct)
    {
        var templates = await _context.ReviewTemplates
            .Where(t => !t.IsDeleted)
            .ToListAsync(ct);

        var list = templates.Select(t => new {
            id = t.Id.ToString(),
            name = t.Name,
            description = t.Description,
            questionCount = t.QuestionCount,
            status = t.Status
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = list });
    }

    [HttpPost("performance/settings/templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveReviewTemplate([FromBody] CreateReviewTemplateRequest request, CancellationToken ct)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await _context.ReviewTemplates.FindAsync(new object[] { request.Id.Value }, ct);
            if (existing != null)
            {
                existing.Name = request.Name;
                existing.Description = request.Description;
                existing.QuestionCount = request.QuestionCount;
                existing.Status = request.Status;
                existing.LastModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = existing });
            }
        }

        var template = new ReviewTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            QuestionCount = request.QuestionCount,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ReviewTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = template });
    }

    [HttpDelete("performance/settings/templates/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteReviewTemplate(Guid id, CancellationToken ct)
    {
        var existing = await _context.ReviewTemplates.FindAsync(new object[] { id }, ct);
        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = true });
    }

    // ─── BENEFITS ────────────────────────────────────────────────────────────

    [HttpGet("benefits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBenefits(CancellationToken ct)
    {
        var benefits = await _context.BenefitTypes
            .Where(b => !b.IsDeleted)
            .ToListAsync(ct);

        var list = benefits.Select(b => new {
            id = b.Id.ToString(),
            name = b.Name,
            category = b.Category.ToLower(), // health, financial, wellness, professional, other
            description = b.Name + " benefit plan.",
            provider = "Internal/HMO",
            cost = b.AffectsPayroll ? 15000 : 0,
            frequency = "monthly",
            eligibility = "All Staff",
            enrolledCount = _context.EmployeeBenefits.Count(eb => eb.BenefitTypeId == b.Id && eb.Status == "Active"),
            status = b.IsActive ? "active" : "inactive",
            effectiveDate = b.CreatedAt.ToString("yyyy-MM-dd")
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = list });
    }

    [HttpPost("benefits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateBenefit([FromBody] CreateBenefitRequest request, CancellationToken ct)
    {
        var plan = new BenefitType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Category = request.Category,
            AffectsPayroll = request.AffectsPayroll,
            IsTaxable = request.IsTaxable,
            IsActive = true
        };

        await _context.BenefitTypes.AddAsync(plan, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<BenefitType> { StatusCode = 200, Message = "Benefit plan created successfully", Data = plan });
    }

    [HttpGet("benefits/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrollments(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .Where(s => !s.IsDeleted)
            .ToListAsync(ct);

        var enrollments = await _context.EmployeeBenefits
            .Include(eb => eb.BenefitType)
            .Where(eb => !eb.IsDeleted)
            .ToListAsync(ct);

        var response = enrollments.Select(e => {
            var staff = staffList.FirstOrDefault(s => s.UserId == e.EmployeeId || s.Id == e.EmployeeId);
            return new {
                id = e.Id.ToString(),
                employeeId = e.EmployeeId.ToString(),
                employeeName = staff?.User?.FullName ?? "Unknown",
                department = staff?.User?.Department?.Name ?? "Unassigned",
                benefitName = e.BenefitType?.Name ?? "Unknown",
                enrolledDate = e.StartDate.ToString("yyyy-MM-dd"),
                status = e.Status.ToLower() // enrolled, pending, waived
            };
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("benefits/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnrollEmployee([FromBody] EnrollBenefitRequest request, CancellationToken ct)
    {
        var enrollment = new EmployeeBenefit
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            BenefitTypeId = request.BenefitTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AmountOrValue = request.AmountOrValue,
            Status = "Active"
        };

        await _context.EmployeeBenefits.AddAsync(enrollment, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<EmployeeBenefit> { StatusCode = 200, Message = "Employee enrolled successfully", Data = enrollment });
    }

    // ─── PERFORMANCE ─────────────────────────────────────────────────────────

    [HttpGet("performance/cycles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCycles(CancellationToken ct)
    {
        var cycles = await _context.PerformanceCycles
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);

        var response = cycles.Select(c => new {
            id = c.Id.ToString(),
            name = c.Name,
            period = c.Name.Contains("Q") ? c.Name.Substring(c.Name.IndexOf("Q")) : "Q3 2026",
            startDate = c.StartDate.ToString("yyyy-MM-dd"),
            endDate = c.EndDate.ToString("yyyy-MM-dd"),
            employeeCount = _context.StaffProfiles.Count(s => !s.IsDeleted),
            status = c.Status // Draft, Active, Completed
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("performance/cycles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCycle([FromBody] CreateCycleRequest request, CancellationToken ct)
    {
        var cycle = new PerformanceCycle
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status
        };

        await _context.PerformanceCycles.AddAsync(cycle, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<PerformanceCycle> { StatusCode = 200, Message = "Performance cycle created successfully", Data = cycle });
    }

    [HttpGet("performance/goals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoals(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .ToListAsync(ct);

        var goals = await _context.PerformanceGoals
            .Include(g => g.Cycle)
            .Where(g => !g.IsDeleted)
            .ToListAsync(ct);

        var response = goals.Select(g => {
            var staff = staffList.FirstOrDefault(s => s.UserId == g.EmployeeId || s.Id == g.EmployeeId);
            return new {
                id = g.Id.ToString(),
                title = g.Title,
                employee = staff?.User?.FullName ?? "Unknown",
                employeeId = g.EmployeeId.ToString(),
                type = "Operational",
                period = g.Cycle?.Name ?? "Q3 2026",
                progress = g.ProgressPercentage,
                confidence = g.ProgressPercentage >= 80 ? "High" : g.ProgressPercentage >= 50 ? "On Track" : g.ProgressPercentage >= 30 ? "At Risk" : "Behind",
                status = g.Status, // Draft, Active, Completed, Deferred
                reviewer = "Ayodele Ayowole"
            };
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("performance/goals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request, CancellationToken ct)
    {
        var goal = new PerformanceGoal
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            CycleId = request.CycleId,
            Title = request.Title,
            Description = request.Description,
            ProgressPercentage = request.ProgressPercentage,
            Status = request.Status
        };

        await _context.PerformanceGoals.AddAsync(goal, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<PerformanceGoal> { StatusCode = 200, Message = "Goal created successfully", Data = goal });
    }

    [HttpGet("performance/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .Where(s => !s.IsDeleted)
            .ToListAsync(ct);

        var reviews = await _context.PerformanceReviews
            .Include(r => r.Cycle)
            .Where(r => !r.IsDeleted)
            .ToListAsync(ct);

        var response = reviews.Select(r => {
            var staff = staffList.FirstOrDefault(s => s.UserId == r.EmployeeId || s.Id == r.EmployeeId);
            var reviewerUser = _context.Users.FirstOrDefault(u => u.Id == r.ReviewerId);
            int ratingVal = (int)Math.Round(r.Score);
            return new {
                id = r.Id.ToString(),
                employeeId = r.EmployeeId.ToString(),
                employeeName = staff?.User?.FullName ?? "Unknown",
                department = staff?.User?.Department?.Name ?? "Unassigned",
                reviewPeriod = r.Cycle?.Name ?? "Q3 2026",
                reviewType = "mid-year",
                reviewer = reviewerUser?.FullName ?? "Ayodele Ayowole",
                status = r.Status.ToLower(), // draft, in-progress, completed, overdue
                dueDate = r.Cycle?.EndDate.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                completedDate = r.LastModifiedAt?.ToString("yyyy-MM-dd"),
                overallRating = ratingVal,
                scores = new {
                    jobKnowledge = ratingVal,
                    workQuality = ratingVal,
                    punctuality = ratingVal,
                    teamwork = ratingVal,
                    initiative = ratingVal,
                    communication = ratingVal
                },
                comments = r.ReviewerNotes
            };
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("performance/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var review = new PerformanceReview
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ReviewerId = request.ReviewerId,
            CycleId = request.CycleId,
            Score = request.Score,
            ReviewerNotes = request.ReviewerNotes,
            EmployeeComments = request.EmployeeComments,
            Status = request.Status
        };

        await _context.PerformanceReviews.AddAsync(review, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<PerformanceReview> { StatusCode = 200, Message = "Review created successfully", Data = review });
    }

    // ─── DISCIPLINARY ────────────────────────────────────────────────────────

    [HttpGet("disciplinary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDisciplinary(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .Where(s => !s.IsDeleted)
            .ToListAsync(ct);

        var cases = await _context.DisciplinaryCases
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);

        var response = cases.Select(c => {
            var staff = staffList.FirstOrDefault(s => s.UserId == c.EmployeeId || s.Id == c.EmployeeId);
            var hrOfficerUser = _context.Users.FirstOrDefault(u => u.Id == c.HrOfficerId);
            return new {
                id = c.Id.ToString(),
                employeeId = c.EmployeeId.ToString(),
                employeeName = staff?.User?.FullName ?? "Unknown",
                department = staff?.User?.Department?.Name ?? "Unassigned",
                offence = c.Description,
                category = c.IncidentType,
                severity = c.IsConfidential ? "gross-misconduct" : "minor",
                status = c.Status.ToLower().Replace(" ", "-"), // open, under-investigation, hearing-scheduled, resolved
                reportedDate = c.IncidentDate.ToString("yyyy-MM-dd"),
                reportedBy = hrOfficerUser?.FullName ?? "HR Department",
                hearingDate = c.HearingDate?.ToString("yyyy-MM-dd"),
                outcome = c.Outcome,
                sanction = c.ActionTaken,
                notes = c.Description
            };
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("disciplinary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDisciplinaryCase([FromBody] CreateDisciplinaryCaseRequest request, CancellationToken ct)
    {
        var caseRecord = new DisciplinaryCase
        {
            Id = Guid.NewGuid(),
            CaseReference = "DC-" + DateTime.UtcNow.Ticks.ToString().Substring(10),
            EmployeeId = request.EmployeeId,
            HrOfficerId = Guid.Empty, // Default or mock HR officer
            IncidentType = request.Category,
            IncidentDate = request.IncidentDate,
            Description = request.Description,
            Status = "Open",
            IsConfidential = request.Severity == "gross-misconduct"
        };

        await _context.DisciplinaryCases.AddAsync(caseRecord, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<DisciplinaryCase> { StatusCode = 200, Message = "Disciplinary case opened successfully", Data = caseRecord });
    }

    [HttpPost("disciplinary/{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveDisciplinaryCase(Guid id, [FromBody] ResolveDisciplinaryCaseRequest request, CancellationToken ct)
    {
        var caseRecord = await _context.DisciplinaryCases
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (caseRecord == null)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Case not found" });

        caseRecord.Status = "Resolved";
        caseRecord.Outcome = request.OutcomeStatus;
        caseRecord.ActionTaken = request.ActionTaken;

        await _context.SaveChangesAsync(ct);

        return Ok(new Response<DisciplinaryCase> { StatusCode = 200, Message = "Case resolved successfully", Data = caseRecord });
    }

    // ─── EXITS ───────────────────────────────────────────────────────────────

    [HttpGet("exit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExits(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .Where(s => !s.IsDeleted)
            .ToListAsync(ct);

        var exits = await _context.ExitRecords
            .Where(e => !e.IsDeleted)
            .ToListAsync(ct);

        var response = exits.Select(e => {
            var staff = staffList.FirstOrDefault(s => s.UserId == e.EmployeeId || s.Id == e.EmployeeId);
            return new {
                id = e.Id.ToString(),
                employeeId = e.EmployeeId.ToString(),
                employeeName = staff?.User?.FullName ?? "Unknown",
                department = staff?.User?.Department?.Name ?? "Unassigned",
                jobTitle = staff?.User?.JobTitle ?? "Officer",
                exitReason = e.ExitType.ToLower(), // resignation, retirement, termination, etc.
                noticeDate = e.NoticeDate.ToString("yyyy-MM-dd"),
                lastWorkingDate = e.LastWorkingDate?.ToString("yyyy-MM-dd") ?? e.ExitDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                exitInterviewDone = e.HandoverNotes != null,
                handoverDone = e.HandoverToEmployeeId != null,
                itClearance = e.Status == "Completed",
                hrClearance = e.Status == "Completed",
                financeClearance = e.Status == "Completed",
                status = e.Status.ToLower() == "completed" ? "completed" : e.Status.ToLower() == "approved" ? "clearance-pending" : "notice-given",
                notes = e.Reason,
                interviewNotes = e.HandoverNotes
            };
        });

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = response });
    }

    [HttpPost("exit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateExitRecord([FromBody] CreateExitRequest request, CancellationToken ct)
    {
        var record = new ExitRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ExitType = request.ExitType,
            NoticeDate = request.NoticeDate,
            LastWorkingDate = request.LastWorkingDate,
            Reason = request.Reason,
            Status = "Notice Given"
        };

        await _context.ExitRecords.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new Response<ExitRecord> { StatusCode = 200, Message = "Exit record saved successfully", Data = record });
    }

    [HttpPost("exit/{id:guid}/clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteExitProcess(Guid id, [FromBody] ClearExitRequest request, CancellationToken ct)
    {
        var record = await _context.ExitRecords
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (record == null)
            return NotFound(new Response<object> { StatusCode = 404, Message = "Exit record not found" });

        record.Status = "Completed";
        record.HandoverNotes = request.Notes;
        record.HandoverToEmployeeId = Guid.Empty;

        await _context.SaveChangesAsync(ct);

        return Ok(new Response<ExitRecord> { StatusCode = 200, Message = "Exit clearance completed successfully", Data = record });
    }
}

// ─── REQUEST MODEL RECORDS ───────────────────────────────────────────────────

public record CreateBenefitRequest(string Name, string Code, string Category, bool AffectsPayroll, bool IsTaxable);
public record EnrollBenefitRequest(Guid EmployeeId, Guid BenefitTypeId, DateTime StartDate, DateTime? EndDate, decimal? AmountOrValue);
public record CreateCycleRequest(string Name, DateTime StartDate, DateTime EndDate, string Status);
public record CreateGoalRequest(Guid EmployeeId, Guid CycleId, string Title, string Description, int ProgressPercentage, string Status);
public record CreateReviewRequest(Guid EmployeeId, Guid ReviewerId, Guid CycleId, decimal Score, string? ReviewerNotes, string? EmployeeComments, string Status);
public record CreateDisciplinaryCaseRequest(Guid EmployeeId, string Category, string Severity, DateTime IncidentDate, string Description, string Notes);
public record ResolveDisciplinaryCaseRequest(string ActionTaken, string OutcomeStatus, string Notes);
public record CreateExitRequest(Guid EmployeeId, string ExitType, DateTime NoticeDate, DateTime LastWorkingDate, string Reason, string Notes);
public record ClearExitRequest(bool ExitInterviewDone, bool HandoverDone, bool ItClearance, bool HrClearance, bool FinanceClearance, string Notes);

public record CreateCompetencyRequest(Guid? Id, string Name, string Description, int SortOrder, string Status);
public record CreateReviewTemplateRequest(Guid? Id, string Name, string Description, int QuestionCount, string Status);
