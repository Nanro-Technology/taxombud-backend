using System;
using FluentValidation;
using TaxOmbud.Application.Hr.DTOs;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Hr.Validators;

public class RequestLoanCommandValidator : AbstractValidator<RequestLoanCommand>
{
    public RequestLoanCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Loan amount must be greater than zero.");
        RuleFor(x => x.TermMonths).GreaterThan(0).WithMessage("Term in months must be greater than zero.");
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(500);
    }
}
