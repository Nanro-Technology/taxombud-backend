using FluentValidation;
using TaxOmbud.Application.Auth.DTOs;

namespace TaxOmbud.Application.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}