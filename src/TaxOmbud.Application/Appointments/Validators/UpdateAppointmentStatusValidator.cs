using FluentValidation;
using TaxOmbud.Application.Appointments.DTOs;

namespace TaxOmbud.Application.Appointments.Validators;

public class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
    }
}