using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.HrRequests.DTOs;
using TaxOmbud.Application.Interfaces.Services;

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

    public HrRequestsController(IHrRequestsService hrRequestsService)
    {
        _hrRequestsService = hrRequestsService;
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
}
