using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Cases.Validators;

public class AssignCaseCommandValidator : AbstractValidator<AssignCaseCommand>
{
    public AssignCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.OfficerId).NotEmpty();
    }
}
