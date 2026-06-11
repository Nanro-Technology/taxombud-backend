using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Taxpayers.Commands.DeactivateTaxpayer;
using TaxOmbud.Application.Features.Taxpayers.Commands.UpdateTaxpayer;
using TaxOmbud.Application.Features.Taxpayers.Commands.VerifyTaxpayer;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetCurrentTaxpayer;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerById;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerComplaints;
using TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayers;
using TaxOmbud.Application.Features.Taxpayers.Queries.VerifyNin;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage taxpayer profiles, verification status, and credentials.
/// </summary>
[Authorize]
[Route("api/v1/taxpayers")]
public class TaxpayersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public TaxpayersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get list of all taxpayer profiles (Officer or above only).</summary>
    [HttpGet]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxpayers(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] bool? isVerified,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTaxpayersQuery(search, type, isVerified, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get taxpayer details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxpayerById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaxpayerByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Update taxpayer profile (either corporate or individual details).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaxpayer(Guid id, [FromBody] UpdateTaxpayerRequest request, CancellationToken ct)
    {
        var command = new UpdateTaxpayerCommand(
            id, request.FirstName, request.LastName, request.Phone, request.TinNumber,
            request.Nin, request.Bvn, request.Gender, request.DateOfBirth,
            request.CompanyName, request.RcNumber, request.Address, request.City, request.State);
            
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Verify/Approve taxpayer verification status.</summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = "OfficerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyTaxpayer(Guid id, [FromBody] VerifyTaxpayerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyTaxpayerCommand(id, request.IsVerified), ct);
        return ToActionResult(result);
    }

    /// <summary>Simulate NIMC / NIBSS verification lookup against external authorities.</summary>
    [HttpPost("verify-nin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyNin([FromBody] NinVerificationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyNinQuery(request.Nin), ct);
        return ToActionResult(result);
    }

    /// <summary>Get the current authenticated taxpayer's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentTaxpayerQuery(), ct);
        return ToActionResult(result);
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
        var result = await _mediator.Send(new GetTaxpayerComplaintsQuery(id, status, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Deactivate a taxpayer profile (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTaxpayer(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateTaxpayerCommand(id), ct);
        return ToActionResult(result);
    }
}

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
