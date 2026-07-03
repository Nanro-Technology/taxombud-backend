using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Cases.Validators;

public class ApproveClosureCommandValidator : AbstractValidator<ApproveClosureCommand>
{
    public ApproveClosureCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Rationale).NotEmpty().MinimumLength(100)
            .WithMessage("Terminal CE approval requires a written rationale of at least 100 characters.");
    }
}
