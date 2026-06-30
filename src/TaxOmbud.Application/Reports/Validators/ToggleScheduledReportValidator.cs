using System;
using FluentValidation;
using TaxOmbud.Application.Reports.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Reports.Validators;

public class ToggleScheduledReportCommandValidator : AbstractValidator<ToggleScheduledReportCommand>
{
    public ToggleScheduledReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}