using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Crm.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>Manage CRM Organizations — list, create, update, delete.</summary>
[ApiController]
[Route("api/v1/organizations")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class OrganizationsController : ControllerBase
{
    private readonly ICrmService _crmService;

    public OrganizationsController(ICrmService crmService)
    {
        _crmService = crmService;
    }

    /// <summary>List all organizations.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Response<System.Collections.Generic.List<OrganizationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations(CancellationToken ct)
    {
        var orgs = await _crmService.GetOrganizationsAsync(new GetOrganizationsQuery(), ct);
        return Ok(new Response<System.Collections.Generic.List<OrganizationDto>>
        {
            StatusCode = 200,
            Message = "Organizations retrieved successfully.",
            Data = orgs
        });
    }

    /// <summary>Get a single organization by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetOrganizationById")]
    [ProducesResponseType(typeof(Response<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationById(Guid id, CancellationToken ct)
    {
        try
        {
            var org = await _crmService.GetOrganizationByIdAsync(new GetOrganizationByIdQuery(id), ct);
            return Ok(new Response<OrganizationDto> { StatusCode = 200, Message = "Organization retrieved.", Data = org });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Create a new organization.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new Response<object> { StatusCode = 400, Message = "Organization name is required." });

        var id = await _crmService.CreateOrganizationAsync(
            new CreateOrganizationCommand(request.Name, request.Phone, request.Email, request.PrimaryTaxPayerId), ct);

        return CreatedAtRoute("GetOrganizationById", new { id },
            new Response<Guid> { StatusCode = 201, Message = "Organization created successfully.", Data = id });
    }

    /// <summary>Update an existing organization.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] CreateOrganizationRequest request, CancellationToken ct)
    {
        try
        {
            await _crmService.UpdateOrganizationAsync(
                new UpdateOrganizationCommand(id, request.Name ?? string.Empty, request.Phone, request.Email, request.PrimaryTaxPayerId), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Organization updated successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }

    /// <summary>Delete an organization.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrganization(Guid id, CancellationToken ct)
    {
        try
        {
            await _crmService.DeleteOrganizationAsync(new DeleteOrganizationCommand(id), ct);
            return Ok(new Response<object> { StatusCode = 200, Message = "Organization deleted successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new Response<object> { StatusCode = 404, Message = ex.Message });
        }
    }
}

public record CreateOrganizationRequest(
    string Name,
    string? Phone,
    string? Email,
    Guid? PrimaryTaxPayerId
);
