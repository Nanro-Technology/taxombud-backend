using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Appointments.Commands.BookAppointment;
using TaxOmbud.Application.Features.Appointments.Commands.UpdateAppointment;
using TaxOmbud.Application.Features.Appointments.Commands.UpdateAppointmentStatus;
using TaxOmbud.Application.Features.Appointments.Queries.GetAppointmentById;
using TaxOmbud.Application.Features.Appointments.Queries.GetAppointments;
using TaxOmbud.Application.Features.Appointments.Queries.GetAvailability;
using TaxOmbud.Application.Features.Appointments.Queries.GetCalendar;

namespace TaxOmbud.Api.Controllers;

/// <summary>
/// Book, view, and schedule meetings/appointments between taxpayers and OTO case officers.
/// </summary>
[Authorize]
[Route("api/v1/appointments")]
public class AppointmentsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List appointments (optionally filtered by taxpayer, officer, or status).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] Guid? taxpayerId,
        [FromQuery] Guid? officerId,
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAppointmentsQuery(taxpayerId, officerId, status), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single appointment by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAppointmentByIdQuery(id), ct);
        return ToActionResult(result);
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
            
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update an existing appointment details.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
    {
        var command = new UpdateAppointmentCommand(
            id, request.Title, request.Description, request.StartTime, request.EndTime,
            request.Location, request.MeetingUrl);
            
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Update appointment status (Confirm, Cancel, Reject, Reschedule).</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateAppointmentStatusCommand(id, request.Status), ct);
        return ToActionResult(result);
    }

    /// <summary>Check available time slots for a specific officer on a given date.</summary>
    [HttpGet("availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] Guid officerId, 
        [FromQuery] DateTimeOffset date, 
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAvailabilityQuery(officerId, date), ct);
        return ToActionResult(result);
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
        var result = await _mediator.Send(new GetCalendarQuery(officerId, taxpayerId, month, year), ct);
        return ToActionResult(result);
    }
}

public record BookAppointmentRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);

public record UpdateAppointmentRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Location,
    string? MeetingUrl
);

public record UpdateAppointmentStatusRequest(string Status);
