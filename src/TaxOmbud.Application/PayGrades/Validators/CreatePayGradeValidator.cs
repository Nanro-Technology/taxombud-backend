using System;
using FluentValidation;
using TaxOmbud.Application.PayGrades.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.PayGrades.Validators;

public class CreatePayGradeCommandValidator : AbstractValidator<CreatePayGradeCommand>
{
    public CreatePayGradeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).GreaterThanOrEqualTo(0).WithMessage("Pay grade level must be greater than or equal to zero.");
        RuleFor(x => x.BasicSalaryBand).NotEmpty().MaximumLength(100);
    }
}