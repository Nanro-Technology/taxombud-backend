using System;
using FluentValidation;
using TaxOmbud.Application.Hr.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Hr.Validators;

public class SaveStaffProfileCommandValidator : AbstractValidator<SaveStaffProfileCommand>
{
    public SaveStaffProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.EmploymentStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaritalStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BankAccountNo).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BankId).NotEmpty().MaximumLength(50);
    }
}
