using FluentValidation;
using TaxOmbud.Application.Auth.DTOs;

namespace TaxOmbud.Application.Auth.Validators;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
