using System;
using FluentValidation;
using TaxOmbud.Application.Roles.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Roles.Validators;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Scope).Must(s => s.Equals("sitewide", StringComparison.OrdinalIgnoreCase) || s.Equals("private", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Scope must be 'sitewide' or 'private'.");
    }
}