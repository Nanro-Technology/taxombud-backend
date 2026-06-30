using System;
using FluentValidation;
using TaxOmbud.Application.Officers.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Officers.Validators;

public class UpdateOfficerProfileCommandValidator : AbstractValidator<UpdateOfficerProfileCommand>
{
    public UpdateOfficerProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MaxCaseload).GreaterThanOrEqualTo(0).WithMessage("Max caseload must be greater than or equal to zero.");
        RuleFor(x => x.EmployeeNumber).MaximumLength(50);
        RuleFor(x => x.Specialisation).MaximumLength(200);
    }
}