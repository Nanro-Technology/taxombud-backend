using FluentValidation;
using TaxOmbud.Application.Auth.DTOs;

namespace TaxOmbud.Application.Auth.Validators;

public class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}
