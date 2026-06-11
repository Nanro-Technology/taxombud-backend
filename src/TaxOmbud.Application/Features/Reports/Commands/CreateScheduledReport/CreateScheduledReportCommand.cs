using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.Reports.Commands.CreateScheduledReport;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateScheduledReportCommand(
    string ReportName,
    string CronExpression,
    string[] Recipients,
    string? Format
) : IRequest<Result<CreatedScheduledReportResponse>>;

public record CreatedScheduledReportResponse(
    Guid Id,
    string ReportName,
    string CronExpression,
    string Format
);

// ─── Validator ────────────────────────────────────────────────────────────────

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateScheduledReportCommandHandler : IRequestHandler<CreateScheduledReportCommand, Result<CreatedScheduledReportResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateScheduledReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreatedScheduledReportResponse>> Handle(CreateScheduledReportCommand request, CancellationToken cancellationToken)
    {
        var report = new ScheduledReport
        {
            Id = Guid.NewGuid(),
            ReportName = request.ReportName,
            CronExpression = request.CronExpression,
            Recipients = string.Join(",", request.Recipients),
            Format = request.Format ?? "CSV",
            IsActive = true
        };

        _context.ScheduledReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreatedScheduledReportResponse(
            report.Id,
            report.ReportName,
            report.CronExpression,
            report.Format
        );

        return Result<CreatedScheduledReportResponse>.Success(response);
    }
}
