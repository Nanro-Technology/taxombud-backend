using System;
using FluentValidation;
using TaxOmbud.Application.Hr.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Hr.Validators;

public class WithdrawEwaCommandValidator : AbstractValidator<WithdrawEwaCommand>
{
    public WithdrawEwaCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero.");
    }
}
