using System;
using FluentValidation;
using TaxOmbud.Application.Reports.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Reports.Validators;

public class CreateScheduledReportCommandValidator : AbstractValidator<CreateScheduledReportCommand>
{
    public CreateScheduledReportCommandValidator()
    {
        RuleFor(x => x.ReportName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CronExpression).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Recipients).NotEmpty().WithMessage("At least one recipient is required.");
        RuleForEach(x => x.Recipients).EmailAddress().WithMessage("Each recipient must be a valid email address.");
    }
}