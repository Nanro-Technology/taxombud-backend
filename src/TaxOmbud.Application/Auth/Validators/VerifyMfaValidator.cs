using FluentValidation;
using TaxOmbud.Application.Auth.DTOs;

namespace TaxOmbud.Application.Auth.Validators;

public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.TotpCode).NotEmpty().Length(6).Matches(@"^\d{6}$");
    }
}
