using System;
using FluentValidation;
using TaxOmbud.Application.System.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.System.Validators;

public class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
