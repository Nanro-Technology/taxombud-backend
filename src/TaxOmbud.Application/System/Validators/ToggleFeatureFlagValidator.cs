using System;
using FluentValidation;
using TaxOmbud.Application.System.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.System.Validators;

public class ToggleFeatureFlagCommandValidator : AbstractValidator<ToggleFeatureFlagCommand>
{
    public ToggleFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
