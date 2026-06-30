using System;
using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Cases.Validators;

public class TransitionCaseCommandValidator : AbstractValidator<TransitionCaseCommand>
{
    public TransitionCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.TargetStage).NotEmpty().MaximumLength(50);
    }
}