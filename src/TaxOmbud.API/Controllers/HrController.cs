using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Hr.Commands.ApproveLeave;
using TaxOmbud.Application.Features.Hr.Commands.ApproveLoan;
using TaxOmbud.Application.Features.Hr.Commands.CreatePayrollRun;
using TaxOmbud.Application.Features.Hr.Commands.RequestLeave;
using TaxOmbud.Application.Features.Hr.Commands.RequestLoan;
using TaxOmbud.Application.Features.Hr.Commands.SaveStaffProfile;
using TaxOmbud.Application.Features.Hr.Commands.WithdrawEwa;
using TaxOmbud.Application.Features.Hr.Queries.GetLeaveRequests;
using TaxOmbud.Application.Features.Hr.Queries.GetPayrollPeriods;
using TaxOmbud.Application.Features.Hr.Queries.GetStaff;
using TaxOmbud.Application.Features.Hr.Queries.GetStaffById;
using TaxOmbud.Application.Features.Hr.Queries.GetWallet;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage HR directory, leave management, payroll validation & runs, salary profiles, employee wallets, and financial requests.
/// </summary>
[Authorize]
[Route("api/v1/hr")]
public class HrController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public HrController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List all staff profiles with pagination and search.</summary>
    [HttpGet("staff")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> GetStaff(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStaffQuery(search, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a specific staff profile by ID.</summary>
    [HttpGet("staff/{id:guid}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStaffByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create or update staff profile.</summary>
    [HttpPost("staff")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveStaffProfile([FromBody] SaveStaffProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SaveStaffProfileCommand(
            request.UserId,
            request.HireDate,
            request.EmploymentStatus,
            request.DateOfBirth,
            request.Nationality,
            request.MaritalStatus,
            request.EmergencyContact,
            request.BankAccountNo,
            request.BankId,
            request.NextOfKin
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>List leave requests.</summary>
    [HttpGet("leave")]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] Guid? userId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(userId, status), ct);
        return ToActionResult(result);
    }

    /// <summary>Submit a leave request.</summary>
    [HttpPost("leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RequestLeaveCommand(
            request.LeaveType,
            request.StartDate,
            request.EndDate
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>Approve/Reject leave request.</summary>
    [HttpPut("leave/{id:guid}/approve")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLeave(Guid id, [FromBody] ApproveLeaveRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveLeaveCommand(
            id,
            request.Approved,
            request.SupervisorNote
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>Get current employee's wallet details and transaction logs.</summary>
    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWalletQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Request Earned Wage Access (EWA).</summary>
    [HttpPost("wallet/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WithdrawEwa([FromBody] EwaWithdrawalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new WithdrawEwaCommand(request.Amount), ct);
        return ToActionResult(result);
    }

    /// <summary>Request an employee loan.</summary>
    [HttpPost("loans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestLoan([FromBody] RequestLoanRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RequestLoanCommand(
            request.Amount,
            request.TermMonths,
            request.Purpose
        ), ct);

        return ToActionResult(result);
    }

    /// <summary>Approve/Reject loan request.</summary>
    [HttpPut("loans/{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] ApproveLoanRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveLoanCommand(id, request.Approved), ct);
        return ToActionResult(result);
    }

    /// <summary>List payroll periods.</summary>
    [HttpGet("payroll/periods")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> GetPayrollPeriods(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollPeriodsQuery(), ct);
        return ToActionResult(result);
    }

    /// <summary>Create and run payroll for a specific period.</summary>
    [HttpPost("payroll/runs")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayrollRun([FromBody] CreatePayrollRunRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreatePayrollRunCommand(request.PeriodId), ct);
        return ToActionResult(result);
    }
}

public record SaveStaffProfileRequest(
    Guid UserId,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    DateTimeOffset DateOfBirth,
    string Nationality,
    string MaritalStatus,
    string EmergencyContact,
    string BankAccountNo,
    string BankId,
    string NextOfKin
);

public record RequestLeaveRequest(string LeaveType, DateTimeOffset StartDate, DateTimeOffset EndDate);

public record ApproveLeaveRequest(bool Approved, string? SupervisorNote);

public record EwaWithdrawalRequest(decimal Amount);

public record RequestLoanRequest(decimal Amount, int TermMonths, string Purpose);

public record ApproveLoanRequest(bool Approved);

public record CreatePayrollRunRequest(Guid PeriodId);
