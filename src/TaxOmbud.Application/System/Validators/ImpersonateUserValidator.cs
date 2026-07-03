using System;
using FluentValidation;
using TaxOmbud.Application.System.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.System.Validators;

public class ImpersonateUserCommandValidator : AbstractValidator<ImpersonateUserCommand>
{
    public ImpersonateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
