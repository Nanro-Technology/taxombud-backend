using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Commands.CreatePayrollRun;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreatePayrollRunCommand(Guid PeriodId) : IRequest<Result<PayrollRun>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreatePayrollRunCommandValidator : AbstractValidator<CreatePayrollRunCommand>
{
    public CreatePayrollRunCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreatePayrollRunCommandHandler : IRequestHandler<CreatePayrollRunCommand, Result<PayrollRun>>
{
    private readonly IApplicationDbContext _context;

    public CreatePayrollRunCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PayrollRun>> Handle(CreatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == request.PeriodId, cancellationToken);
        if (period == null)
        {
            return Result<PayrollRun>.Failure("Payroll period not found.");
        }

        var payrollRun = new PayrollRun
        {
            Id = Guid.NewGuid(),
            PeriodId = request.PeriodId,
            Status = "draft"
        };

        _context.PayrollRuns.Add(payrollRun);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PayrollRun>.Success(payrollRun);
    }
}
