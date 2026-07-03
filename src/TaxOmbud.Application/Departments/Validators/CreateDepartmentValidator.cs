using System;
using FluentValidation;
using TaxOmbud.Application.Departments.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Departments.Validators;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoutingMode).Must(m => m.Equals("head", StringComparison.OrdinalIgnoreCase) || m.Equals("members", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Routing mode must be 'head' or 'members'.");
    }
}
