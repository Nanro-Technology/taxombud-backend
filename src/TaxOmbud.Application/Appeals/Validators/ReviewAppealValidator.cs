using FluentValidation;
using TaxOmbud.Application.Appeals.DTOs;

namespace TaxOmbud.Application.Appeals.Validators;

public class ReviewAppealCommandValidator : AbstractValidator<ReviewAppealCommand>
{
    public ReviewAppealCommandValidator()
    {
        RuleFor(x => x.AppealId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(2000);
    }
}