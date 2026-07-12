using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TaxOmbud.Application.Hr.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage HR directory, leave management, payroll validation and runs, salary profiles, employee wallets, and financial requests.
/// </summary>
[ApiController]
[Route("api/v1/hr")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class HrController : ControllerBase
{
    private readonly IHrService _hrService;
    private readonly IApplicationDbContext _context;

    public HrController(IHrService hrService, IApplicationDbContext context)
    {
        _hrService = hrService;
        _context = context;
    }

    /// <summary>Get HR overview statistics and metrics.</summary>
    [HttpGet("overview/stats")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverviewStats(CancellationToken ct)
    {
        var staffList = await _context.StaffProfiles
            .Include(sp => sp.User)
            .ToListAsync(ct);

        var activeStaff = staffList.Count(s => s.EmploymentStatus == "Active" || s.EmploymentStatus == "Full-Time" || s.EmploymentStatus == "Contract");
        var inactive = staffList.Count(s => s.EmploymentStatus == "Terminated" || s.EmploymentStatus == "Resigned" || s.EmploymentStatus == "Retired");

        var leaves = await _context.LeaveRequests
            .Include(l => l.User)
            .ToListAsync(ct);

        var onLeave = leaves.Count(l => l.Status == "Approved" && DateTime.UtcNow >= l.StartDate && DateTime.UtcNow <= l.EndDate);
        var pendingLeave = leaves.Count(l => l.Status == "Pending");

        var departments = await _context.Departments.ToListAsync(ct);
        var deptHeadcount = departments.ToDictionary(
            d => d.Name,
            d => staffList.Count(s => s.User?.DepartmentId == d.Id)
        );

        var leaveTypeMix = leaves
            .GroupBy(l => l.LeaveType)
            .ToDictionary(g => g.Key, g => g.Count());

        var leaveTrend = Enumerable.Range(0, 6)
            .Select(i => DateTime.UtcNow.AddMonths(-i))
            .Reverse()
            .Select(date => new
            {
                month = date.ToString("MMM yyyy"),
                days = leaves
                    .Where(l => l.Status == "Approved" && l.StartDate.Month == date.Month && l.StartDate.Year == date.Year)
                    .Sum(l => (l.EndDate - l.StartDate).Days + 1)
            })
            .ToList();

        var leaveRequestsList = leaves.Select(l => new
        {
            l.Id,
            employeeName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown Employee",
            leaveType = l.LeaveType,
            startDate = l.StartDate.ToString("yyyy-MM-dd"),
            endDate = l.EndDate.ToString("yyyy-MM-dd"),
            duration = (l.EndDate - l.StartDate).Days + 1,
            status = l.Status
        }).ToList();

        var stats = new
        {
            activeStaff,
            onLeave,
            inactive,
            pendingLeave,
            deptHeadcount,
            leaveTypeMix,
            leaveTrend,
            leaveRequests = leaveRequestsList
        };

        return Ok(new Response<object> { StatusCode = 200, Message = "Success", Data = stats });
    }

    /// <summary>List all staff profiles with pagination and search.</summary>
    [HttpGet("staff")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaff(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _hrService.GetStaffAsync(new GetStaffQuery(search, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a specific staff profile by ID.</summary>
    [HttpGet("staff/{id:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffById(Guid id, CancellationToken ct)
    {
        var result = await _hrService.GetStaffByIdAsync(new GetStaffByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create or update staff profile.</summary>
    [HttpPost("staff")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveStaffProfile([FromBody] SaveStaffProfileRequest request, CancellationToken ct)
    {
        var result = await _hrService.SaveStaffProfileAsync(new SaveStaffProfileCommand(
            request.UserId, request.EmployeeCode, request.Title, request.SupervisorId,
            request.HireDate, request.EmploymentStatus, request.DateOfBirth,
            request.Nationality, request.MaritalStatus, request.EducationLevel, request.EducationDetails,
            request.AddressLine1, request.AddressLine2, request.City, request.State, request.Country,
            request.EmergencyContactName, request.EmergencyContactPhone,
            request.BankAccountNo, request.BankId,
            request.NextOfKinName, request.NextOfKinRelationship, request.NextOfKinPhone, request.NextOfKinAddress
        ), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>List leave requests.</summary>
    [HttpGet("leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] Guid? userId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _hrService.GetLeaveRequestsAsync(new GetLeaveRequestsQuery(userId, status), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Submit a leave request.</summary>
    [HttpPost("leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveRequest request, CancellationToken ct)
    {
        var result = await _hrService.RequestLeaveAsync(new RequestLeaveCommand(request.LeaveType, request.StartDate, request.EndDate), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Approve/Reject leave request.</summary>
    [HttpPut("leave/{id:guid}/approve")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLeave(Guid id, [FromBody] ApproveLeaveRequest request, CancellationToken ct)
    {
        var result = await _hrService.ApproveLeaveAsync(new ApproveLeaveCommand(id, request.Approved, request.SupervisorNote), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get current employee's wallet details and transaction logs.</summary>
    [HttpGet("wallet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWallet(CancellationToken ct)
    {
        var result = await _hrService.GetWalletAsync(new GetWalletQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Request Earned Wage Access (EWA).</summary>
    [HttpPost("wallet/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WithdrawEwa([FromBody] EwaWithdrawalRequest request, CancellationToken ct)
    {
        var result = await _hrService.WithdrawEwaAsync(new WithdrawEwaCommand(request.Amount), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Request an employee loan.</summary>
    [HttpPost("loans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestLoan([FromBody] RequestLoanRequest request, CancellationToken ct)
    {
        var result = await _hrService.RequestLoanAsync(new RequestLoanCommand(request.Amount, request.TermMonths, request.Purpose), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Approve/Reject loan request.</summary>
    [HttpPut("loans/{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] ApproveLoanRequest request, CancellationToken ct)
    {
        var result = await _hrService.ApproveLoanAsync(new ApproveLoanCommand(id, request.Approved), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>List payroll periods.</summary>
    [HttpGet("payroll/periods")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollPeriods(CancellationToken ct)
    {
        var result = await _hrService.GetPayrollPeriodsAsync(new GetPayrollPeriodsQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create and run payroll for a specific period.</summary>
    [HttpPost("payroll/runs")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayrollRun([FromBody] CreatePayrollRunRequest request, CancellationToken ct)
    {
        var result = await _hrService.CreatePayrollRunAsync(new CreatePayrollRunCommand(request.PeriodId), ct);
        return StatusCode(result.StatusCode, result);
    }

    // ── Competencies ──────────────────────────────────────────────────────────

    /// <summary>List all performance competencies.</summary>
    [HttpGet("performance-settings/competencies")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompetencies(CancellationToken ct)
    {
        var result = await _hrService.GetCompetenciesAsync(new GetCompetenciesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new competency.</summary>
    [HttpPost("performance-settings/competencies")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCompetency([FromBody] CreateCompetencyRequest request, CancellationToken ct)
    {
        var result = await _hrService.CreateCompetencyAsync(new CreateCompetencyCommand(request.Name, request.Description, request.SortOrder), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update an existing competency.</summary>
    [HttpPut("performance-settings/competencies/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCompetency(Guid id, [FromBody] UpdateCompetencyRequest request, CancellationToken ct)
    {
        var result = await _hrService.UpdateCompetencyAsync(new UpdateCompetencyCommand(id, request.Name, request.Description, request.SortOrder, request.Status), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a competency.</summary>
    [HttpDelete("performance-settings/competencies/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCompetency(Guid id, CancellationToken ct)
    {
        var result = await _hrService.DeleteCompetencyAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ── Review Templates ──────────────────────────────────────────────────────

    /// <summary>List all review templates.</summary>
    [HttpGet("performance-settings/templates")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewTemplates(CancellationToken ct)
    {
        var result = await _hrService.GetReviewTemplatesAsync(new GetReviewTemplatesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new review template.</summary>
    [HttpPost("performance-settings/templates")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReviewTemplate([FromBody] CreateReviewTemplateRequest request, CancellationToken ct)
    {
        var result = await _hrService.CreateReviewTemplateAsync(new CreateReviewTemplateCommand(request.Name, request.Description, request.QuestionCount), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update a review template.</summary>
    [HttpPut("performance-settings/templates/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReviewTemplate(Guid id, [FromBody] UpdateReviewTemplateRequest request, CancellationToken ct)
    {
        var result = await _hrService.UpdateReviewTemplateAsync(new UpdateReviewTemplateCommand(id, request.Name, request.Description, request.QuestionCount, request.Status), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a review template.</summary>
    [HttpDelete("performance-settings/templates/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReviewTemplate(Guid id, CancellationToken ct)
    {
        var result = await _hrService.DeleteReviewTemplateAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ── Performance Cycles ────────────────────────────────────────────────────

    /// <summary>List all performance cycles.</summary>
    [HttpGet("performance-settings/cycles")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceCycles(CancellationToken ct)
    {
        var result = await _hrService.GetPerformanceCyclesAsync(new GetPerformanceCyclesQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new performance cycle.</summary>
    [HttpPost("performance-settings/cycles")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePerformanceCycle([FromBody] CreatePerformanceCycleRequest request, CancellationToken ct)
    {
        var result = await _hrService.CreatePerformanceCycleAsync(new CreatePerformanceCycleCommand(request.Name, request.StartDate, request.EndDate), ct);
        return StatusCode(result.StatusCode, result);
    }

    // ── Bulk Onboarding ───────────────────────────────────────────────────────

    /// <summary>Bulk onboard multiple employees at once.</summary>
    [HttpPost("bulk-onboarding")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkOnboard([FromBody] BulkOnboardRequest request, CancellationToken ct)
    {
        if (request.Employees == null || !request.Employees.Any())
            return BadRequest(new Response<object> { StatusCode = 400, Message = "No employees provided." });

        var result = await _hrService.BulkOnboardAsync(request, ct);
        return StatusCode(result.StatusCode, result);
    }
}

