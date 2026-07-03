using System;
using FluentValidation;
using TaxOmbud.Application.PayGrades.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.PayGrades.Validators;

public class SaveSalaryProfileCommandValidator : AbstractValidator<SaveSalaryProfileCommand>
{
    public SaveSalaryProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Basic).GreaterThanOrEqualTo(0).WithMessage("Basic salary must be greater than or equal to zero.");
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
