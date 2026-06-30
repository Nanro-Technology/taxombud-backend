using System;
using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Application.Cases.Validators;

public class AssignCaseCommandValidator : AbstractValidator<AssignCaseCommand>
{
    public AssignCaseCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.OfficerId).NotEmpty();
    }
}