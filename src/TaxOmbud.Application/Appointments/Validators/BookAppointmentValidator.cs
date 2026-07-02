using FluentValidation;
using TaxOmbud.Application.Appointments.DTOs;

namespace TaxOmbud.Application.Appointments.Validators;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be earlier than End time.");
    }
}