using System;
using FluentValidation;
using TaxOmbud.Application.Hr.DTOs;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Hr.Validators;

public class RequestLeaveCommandValidator : AbstractValidator<RequestLeaveCommand>
{
    public RequestLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}