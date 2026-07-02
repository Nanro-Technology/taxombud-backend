using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Cases.Validators;

public class TransitionCaseCommandValidator : AbstractValidator<TransitionCaseCommand>
{
    public TransitionCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.TargetStage).NotEmpty().MaximumLength(50);
    }
}