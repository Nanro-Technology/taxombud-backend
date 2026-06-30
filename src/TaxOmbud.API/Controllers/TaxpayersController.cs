using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Taxpayers.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Api.Controllers;

public record UpdateTaxpayerRequest(
    string FirstName,
    string LastName,
    string Phone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State
);
public record VerifyTaxpayerRequest(bool IsVerified);
public record NinVerificationRequest(string Nin);

/// <summary>
/// Manage taxpayer profiles, verification status, and credentials.
/// </summary>
[ApiController]
[Route("api/v1/taxpayers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class TaxpayersController : ControllerBase
{
    private readonly ITaxpayersService _taxpayersService;

    public TaxpayersController(ITaxpayersService taxpayersService)
    {
        _taxpayersService = taxpayersService;
    }

    /// <summary>Get list of all taxpayer profiles (Officer or above only).</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<PagedResult<TaxpayerListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxpayers(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] bool? isVerified,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _taxpayersService.GetTaxpayersAsync(new GetTaxpayersQuery(search, type, isVerified, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get taxpayer details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<TaxpayerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxpayerById(Guid id, CancellationToken ct)
    {
        var result = await _taxpayersService.GetTaxpayerByIdAsync(new GetTaxpayerByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single taxpayer profile by TIN.</summary>
    [HttpGet("tin/{tin}")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(typeof(Response<TaxpayerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxpayerByTin(string tin, CancellationToken ct)
    {
        var result = await _taxpayersService.GetTaxpayerByTinAsync(new GetTaxpayerByTinQuery(tin), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get the current authenticated taxpayer's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(Response<TaxpayerDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _taxpayersService.GetCurrentTaxpayerAsync(new GetCurrentTaxpayerQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update taxpayer profile (either corporate or individual details).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaxpayer(Guid id, [FromBody] UpdateTaxpayerRequest request, CancellationToken ct)
    {
        var command = new UpdateTaxpayerCommand(
            id, request.FirstName, request.LastName, request.Phone, request.TinNumber,
            request.Nin, request.Bvn, request.Gender, request.DateOfBirth,
            request.CompanyName, request.RcNumber, request.Address, request.City, request.State);
        var result = await _taxpayersService.UpdateTaxpayerAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Verify/Approve taxpayer verification status.</summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyTaxpayer(Guid id, [FromBody] VerifyTaxpayerRequest request, CancellationToken ct)
    {
        var result = await _taxpayersService.VerifyTaxpayerAsync(new VerifyTaxpayerCommand(id, request.IsVerified), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Simulate NIMC / NIBSS verification lookup against external authorities.</summary>
    [HttpPost("verify-nin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyNin([FromBody] NinVerificationRequest request, CancellationToken ct)
    {
        var result = await _taxpayersService.VerifyNinAsync(new VerifyNinQuery(request.Nin), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get list of complaints submitted by a specific taxpayer.</summary>
    [HttpGet("{id:guid}/complaints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxpayerComplaints(
        Guid id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _taxpayersService.GetTaxpayerComplaintsAsync(new GetTaxpayerComplaintsQuery(id, status, page, pageSize), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Deactivate (soft-delete) a taxpayer account (Admin only).</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTaxpayer(Guid id, CancellationToken ct)
    {
        var result = await _taxpayersService.DeactivateTaxpayerAsync(new DeactivateTaxpayerCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}
