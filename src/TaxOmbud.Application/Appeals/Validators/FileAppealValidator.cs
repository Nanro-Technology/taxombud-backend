using FluentValidation;
using TaxOmbud.Application.Appeals.DTOs;

namespace TaxOmbud.Application.Appeals.Validators;

public class FileAppealCommandValidator : AbstractValidator<FileAppealCommand>
{
    public FileAppealCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}
