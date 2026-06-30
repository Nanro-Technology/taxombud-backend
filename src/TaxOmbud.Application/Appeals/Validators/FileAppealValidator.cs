using System;
using FluentValidation;
using TaxOmbud.Application.Appeals.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Appeals.Validators;

public class FileAppealCommandValidator : AbstractValidator<FileAppealCommand>
{
    public FileAppealCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}