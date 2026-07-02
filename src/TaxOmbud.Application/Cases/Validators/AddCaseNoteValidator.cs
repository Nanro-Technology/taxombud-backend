using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Cases.Validators;

public class AddCaseNoteCommandValidator : AbstractValidator<AddCaseNoteCommand>
{
    public AddCaseNoteCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}