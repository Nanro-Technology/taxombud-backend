using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.RunPayroll;

public record RunPayrollCommands(Guid PeriodId) : IRequest<Result<Guid>>;

public class RunPayrollCommandsHandler : IRequestHandler<RunPayrollCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public RunPayrollCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(RunPayrollCommands request, CancellationToken cancellationToken)
    {
        var entity = new PayrollRun
        {
            Id = Guid.NewGuid(),
            PeriodId = request.PeriodId,
            Status = "Pending",
            PostedAt = DateTime.UtcNow
        };
        _context.PayrollRuns.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}