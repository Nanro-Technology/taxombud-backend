using System;
using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Cases.Validators;

public class AddCaseNoteCommandValidator : AbstractValidator<AddCaseNoteCommand>
{
    public AddCaseNoteCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}