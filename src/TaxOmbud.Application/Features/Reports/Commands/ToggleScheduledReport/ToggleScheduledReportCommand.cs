using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Commands.ToggleScheduledReport;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ToggleScheduledReportCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ToggleScheduledReportCommandValidator : AbstractValidator<ToggleScheduledReportCommand>
{
    public ToggleScheduledReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ToggleScheduledReportCommandHandler : IRequestHandler<ToggleScheduledReportCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ToggleScheduledReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ToggleScheduledReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.ScheduledReports.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (report == null)
            return Result<Unit>.NotFound("Scheduled report not found.");

        report.IsActive = !report.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
