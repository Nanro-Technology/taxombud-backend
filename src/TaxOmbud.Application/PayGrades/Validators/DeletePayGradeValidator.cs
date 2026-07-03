using System;
using FluentValidation;
using TaxOmbud.Application.PayGrades.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.PayGrades.Validators;

public class DeletePayGradeCommandValidator : AbstractValidator<DeletePayGradeCommand>
{
    public DeletePayGradeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
