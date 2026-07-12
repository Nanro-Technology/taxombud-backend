using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Appointments.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Book, view, and schedule meetings/appointments between taxpayers and OTO case officers.
/// </summary>
[ApiController]
[Route("api/v1/appointments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentsService _appointmentsService;
    private readonly TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext _context;

    public AppointmentsController(IAppointmentsService appointmentsService, TaxOmbud.Application.Interfaces.Persistence.IApplicationDbContext context)
    {
        _appointmentsService = appointmentsService;
        _context = context;
    }

    /// <summary>List appointments (optionally filtered by taxpayer, officer, or status).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] Guid? taxpayerId,
        [FromQuery] Guid? officerId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _appointmentsService.GetAppointmentsAsync(new GetAppointmentsQuery(taxpayerId, officerId, status), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a single appointment by ID.</summary>
    [HttpGet("{id:guid}", Name = "GetAppointmentById")]
    [ProducesResponseType(typeof(Response<AppointmentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentById(Guid id, CancellationToken ct)
    {
        var result = await _appointmentsService.GetAppointmentByIdAsync(new GetAppointmentByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Check available time slots for a specific officer on a given date.</summary>
    [HttpGet("availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] Guid officerId,
        [FromQuery] DateTimeOffset date,
        CancellationToken ct = default)
    {
        var result = await _appointmentsService.GetAvailabilityAsync(new GetAvailabilityQuery(officerId, date), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get a calendar view of appointments for a specific month.</summary>
    [HttpGet("calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] Guid? officerId,
        [FromQuery] Guid? taxpayerId,
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct = default)
    {
        var result = await _appointmentsService.GetCalendarAsync(new GetCalendarQuery(officerId, taxpayerId, month, year), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Book a new meeting/appointment (Taxpayer or Officer action).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request, CancellationToken ct)
    {
        var command = new BookAppointmentCommand(
            request.Title, request.Description, request.StartTime, request.EndTime,
            request.TaxpayerId, request.OfficerId, request.Location, request.MeetingUrl);
        var result = await _appointmentsService.BookAppointmentAsync(command, ct);
        if (!(result.StatusCode >= 200 && result.StatusCode < 300))
            return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update an existing appointment details.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
    {
        var command = new UpdateAppointmentCommand(
            id, request.Title, request.Description, request.StartTime, request.EndTime,
            request.Location, request.MeetingUrl);
        var result = await _appointmentsService.UpdateAppointmentAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update appointment status (Confirm, Cancel, Reject, Reschedule).</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var result = await _appointmentsService.UpdateAppointmentStatusAsync(new UpdateAppointmentStatusCommand(id, request.Status), ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Clear all appointments (Helper endpoint for testing).</summary>
    [HttpDelete("clear-all")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAll(CancellationToken ct)
    {
        _context.Appointments.RemoveRange(_context.Appointments);
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Cleared all appointments successfully." });
    }
}
