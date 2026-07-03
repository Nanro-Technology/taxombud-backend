using FluentValidation;
using TaxOmbud.Application.Users.DTOs;

namespace TaxOmbud.Application.Users.Validators;

public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
