using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Appointments.Commands.BookAppointment;

// ─── Command ─────────────────────────────────────────────────────────────────

public record BookAppointmentCommand(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
) : IRequest<Result<BookAppointmentResponse>>;

public record BookAppointmentResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be earlier than End time.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<BookAppointmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public BookAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BookAppointmentResponse>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = AppointmentStatus.Scheduled,
            TaxpayerId = request.TaxpayerId,
            OfficerId = request.OfficerId,
            Location = request.Location,
            MeetingUrl = request.MeetingUrl
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<BookAppointmentResponse>.Success(new BookAppointmentResponse(
            appointment.Id,
            appointment.Title,
            appointment.Description,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Status.ToString(),
            appointment.TaxpayerId,
            appointment.OfficerId,
            appointment.Location,
            appointment.MeetingUrl
        ));
    }
}
