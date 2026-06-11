using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Appointments.Commands.UpdateAppointment;

public record UpdateAppointmentCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Location,
    string? MeetingUrl
) : IRequest<Result<Unit>>;

public class UpdateAppointmentCommandValidator : AbstractValidator<UpdateAppointmentCommand>
{
    public UpdateAppointmentCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Title).MaximumLength(200).NotEmpty();
        RuleFor(v => v.Description).MaximumLength(1000);
        RuleFor(v => v.StartTime).LessThan(v => v.EndTime).WithMessage("StartTime must be before EndTime.");
        RuleFor(v => v.Location).MaximumLength(200);
        RuleFor(v => v.MeetingUrl).MaximumLength(500);
    }
}

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Appointments
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Appointments.Appointment), request.Id);

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Location = request.Location;
        entity.MeetingUrl = request.MeetingUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
