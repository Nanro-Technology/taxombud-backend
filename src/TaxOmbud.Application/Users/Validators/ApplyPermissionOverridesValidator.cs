using System;
using FluentValidation;
using TaxOmbud.Application.Users.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Users.Validators;

public class ApplyPermissionOverridesCommandValidator : AbstractValidator<ApplyPermissionOverridesCommand>
{
    public ApplyPermissionOverridesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Overrides).NotNull();
        RuleForEach(x => x.Overrides).ChildRules(over =>
        {
            over.RuleFor(x => x.PermissionCode).NotEmpty();
            over.RuleFor(x => x.Mode).Must(m => m.Equals("grant", StringComparison.OrdinalIgnoreCase) || m.Equals("deny", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Mode must be 'grant' or 'deny'.");
        });
    }
}