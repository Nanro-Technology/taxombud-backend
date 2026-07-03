using FluentValidation;
using TaxOmbud.Application.Taxpayers.DTOs;

namespace TaxOmbud.Application.Taxpayers.Validators;

public class VerifyTaxpayerCommandValidator : AbstractValidator<VerifyTaxpayerCommand>
{
    public VerifyTaxpayerCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
    }
}
