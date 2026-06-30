using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Manage dashboard layouts and widgets.
/// </summary>
[Authorize]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public DashboardController(
        IReportsService reportsService
    )
    {
        _reportsService = reportsService;
    }

    /// <summary>Create a new widget (Admin only).</summary>
    [HttpPost("widgets")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult CreateWidget([FromBody] SaveWidgetRequest request, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Update an existing widget (Admin only).</summary>
    [HttpPut("widgets/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult UpdateWidget(Guid id, [FromBody] SaveWidgetRequest request, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Delete a widget (Admin only).</summary>
    [HttpDelete("widgets/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult DeleteWidget(Guid id, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Get the user's customized dashboard layout.</summary>
    [HttpGet("layout")]
    public IActionResult GetLayout(CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }

    /// <summary>Save the user's customized dashboard layout.</summary>
    [HttpPost("layout")]
    public IActionResult SaveLayout([FromBody] SaveDashboardLayoutRequest request, CancellationToken ct)
    {
        return Ok(new { Message = "Not implemented yet" });
    }
}

public record SaveWidgetRequest(string Name, string Description, string ComponentName, string? RequiredPermission, bool IsActive);
public record SaveDashboardLayoutRequest(string LayoutJson);

