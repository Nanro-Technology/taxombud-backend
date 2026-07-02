using FluentValidation;
using TaxOmbud.Application.Auth.DTOs;

namespace TaxOmbud.Application.Auth.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}