using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Reports.Commands.DeleteScheduledReport;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeleteScheduledReportCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class DeleteScheduledReportCommandValidator : AbstractValidator<DeleteScheduledReportCommand>
{
    public DeleteScheduledReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeleteScheduledReportCommandHandler : IRequestHandler<DeleteScheduledReportCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteScheduledReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteScheduledReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.ScheduledReports.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (report == null)
            return Result<Unit>.NotFound("Scheduled report not found.");

        _context.ScheduledReports.Remove(report);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
