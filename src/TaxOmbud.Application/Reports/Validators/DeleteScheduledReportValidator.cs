using System;
using FluentValidation;
using TaxOmbud.Application.Reports.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Reports.Validators;

public class DeleteScheduledReportCommandValidator : AbstractValidator<DeleteScheduledReportCommand>
{
    public DeleteScheduledReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}