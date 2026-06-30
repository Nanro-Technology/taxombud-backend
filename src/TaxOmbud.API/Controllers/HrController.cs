using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Hr.DTOs;
using TaxOmbud.Application.Interfaces.Services;

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

    public HrController(IHrService hrService)
    {
        _hrService = hrService;
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
}
