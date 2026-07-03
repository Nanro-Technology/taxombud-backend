using System;
using FluentValidation;
using TaxOmbud.Application.Hr.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Hr.Validators;

public class ApproveLoanCommandValidator : AbstractValidator<ApproveLoanCommand>
{
    public ApproveLoanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
