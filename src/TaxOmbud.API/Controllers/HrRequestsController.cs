using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.HrRequests.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage HR administrative requests (leave, loans, EWA) from a single management endpoint.
/// </summary>
[ApiController]
[Route("api/v1/hr/requests")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class HrRequestsController : ControllerBase
{
    private readonly IHrRequestsService _hrRequestsService;
    private readonly TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext _context;

    public HrRequestsController(
        IHrRequestsService hrRequestsService, 
        TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext context)
    {
        _hrRequestsService = hrRequestsService;
        _context = context;
    }

    /// <summary>List all leave requests (optionally filtered by status).</summary>
    [HttpGet("leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] GetLeaveRequestsQueries query, CancellationToken ct = default)
    {
        var result = await _hrRequestsService.GetLeaveRequestsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>List all loan requests (optionally filtered by status).</summary>
    [HttpGet("loans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoanRequests([FromQuery] GetLoanRequestsQueries query, CancellationToken ct = default)
    {
        var result = await _hrRequestsService.GetLoanRequestsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>List all EWA requests (optionally filtered by status).</summary>
    [HttpGet("ewa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEwaRequests([FromQuery] GetEwaRequestsQueries query, CancellationToken ct = default)
    {
        var result = await _hrRequestsService.GetEwaRequestsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Submit a leave request.</summary>
    [HttpPost("leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitLeaveRequest([FromBody] SubmitLeaveRequestCommands command, CancellationToken ct)
    {
        var result = await _hrRequestsService.SubmitLeaveRequestAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Approve or reject a leave request.</summary>
    [HttpPost("leaves/approve")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLeaveRequest([FromBody] ApproveLeaveRequestCommands command, CancellationToken ct)
    {
        var result = await _hrRequestsService.ApproveLeaveRequestAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Submit a loan request.</summary>
    [HttpPost("loans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitLoanRequest([FromBody] SubmitLoanRequestCommands command, CancellationToken ct)
    {
        var result = await _hrRequestsService.SubmitLoanRequestAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    // ─── ADMIN FINANCE APPROVALS ──────────────────────────────────────────────

    [HttpPost("loans/{id:guid}/approve")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> ApproveLoan(Guid id, CancellationToken ct)
    {
        var loan = await _context.LoanRequests.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (loan == null) return NotFound("Loan request not found.");

        loan.Status = "approved";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Loan approved successfully." });
    }

    [HttpPost("loans/{id:guid}/reject")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> RejectLoan(Guid id, CancellationToken ct)
    {
        var loan = await _context.LoanRequests.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (loan == null) return NotFound("Loan request not found.");

        loan.Status = "rejected";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Loan rejected successfully." });
    }

    [HttpPost("loans/{id:guid}/disburse")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> DisburseLoan(Guid id, CancellationToken ct)
    {
        var loan = await _context.LoanRequests.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (loan == null) return NotFound("Loan request not found.");

        loan.Status = "disbursed";
        loan.PayoutReference = "LN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        if (loan.DisburseTo?.ToLower() == "wallet")
        {
            var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.UserId == loan.UserId, ct);
            if (wallet != null)
            {
                wallet.BalanceNgn += loan.Amount;
                _context.WalletTransactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Type = "credit",
                    Amount = loan.Amount,
                    Reference = loan.IsSalaryAdvance ? "Salary Advance Disbursal" : "Loan Disbursal",
                    Status = "paid",
                    PaidAt = DateTimeOffset.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Loan disbursed successfully." });
    }

    [HttpPost("ewa")]
    public async Task<IActionResult> SubmitEwaRequest([FromBody] SubmitEwaRequestCommand request, CancellationToken ct)
    {
        var ewa = new EwaRequest
        {
            Id = Guid.NewGuid(),
            UserId = request.StaffId,
            Amount = request.Amount,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.EwaRequests.Add(ewa);
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "EWA request submitted successfully.", Data = ewa.Id });
    }

    [HttpPost("ewa/{id:guid}/approve")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> ApproveEwa(Guid id, CancellationToken ct)
    {
        var ewa = await _context.EwaRequests.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (ewa == null) return NotFound("EWA request not found.");

        ewa.Status = "approved";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "EWA approved successfully." });
    }

    [HttpPost("ewa/{id:guid}/reject")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> RejectEwa(Guid id, CancellationToken ct)
    {
        var ewa = await _context.EwaRequests.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (ewa == null) return NotFound("EWA request not found.");

        ewa.Status = "rejected";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "EWA rejected successfully." });
    }

    [HttpPost("ewa/{id:guid}/disburse")]
    [Authorize(Policy = "OfficerOrAbove")]
    public async Task<IActionResult> DisburseEwa(Guid id, CancellationToken ct)
    {
        var ewa = await _context.EwaRequests.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (ewa == null) return NotFound("EWA request not found.");

        ewa.Status = "disbursed";
        ewa.DisbursedAt = DateTimeOffset.UtcNow;

        var wallet = await _context.EmployeeWallets.FirstOrDefaultAsync(w => w.UserId == ewa.UserId, ct);
        if (wallet != null)
        {
            wallet.BalanceNgn += ewa.Amount;
            _context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = "credit",
                Amount = ewa.Amount,
                Reference = "EWA Disbursal",
                Status = "paid",
                PaidAt = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "EWA disbursed successfully." });
    }

    [HttpPost("leaves/{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeaveRequest(Guid id, CancellationToken ct)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (leave == null) return NotFound("Leave request not found.");

        leave.Status = "Cancelled";
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Leave request cancelled successfully." });
    }
}

public record SubmitEwaRequestCommand(Guid StaffId, decimal Amount);
