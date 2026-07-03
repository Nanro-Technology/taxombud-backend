using System;
using FluentValidation;
using TaxOmbud.Application.PayGrades.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.PayGrades.Validators;

public class UpdatePayGradeCommandValidator : AbstractValidator<UpdatePayGradeCommand>
{
    public UpdatePayGradeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).GreaterThanOrEqualTo(0).WithMessage("Pay grade level must be greater than or equal to zero.");
        RuleFor(x => x.BasicSalaryBand).NotEmpty().MaximumLength(100);
    }
}
