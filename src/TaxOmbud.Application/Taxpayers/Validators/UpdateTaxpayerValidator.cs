using FluentValidation;
using TaxOmbud.Application.Taxpayers.DTOs;

namespace TaxOmbud.Application.Taxpayers.Validators;

public class UpdateTaxpayerCommandValidator : AbstractValidator<UpdateTaxpayerCommand>
{
    public UpdateTaxpayerCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}
