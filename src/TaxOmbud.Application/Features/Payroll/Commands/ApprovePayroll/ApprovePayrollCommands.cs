using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Commands.ApprovePayroll;

public record ApprovePayrollCommands(Guid RunId) : IRequest<Result<bool>>;

public class ApprovePayrollCommandsHandler : IRequestHandler<ApprovePayrollCommands, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ApprovePayrollCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ApprovePayrollCommands request, CancellationToken cancellationToken)
    {
        var run = await _context.PayrollRuns.FirstOrDefaultAsync(x => x.Id == request.RunId, cancellationToken);
        if (run == null) return Result<bool>.NotFound($"Payroll Run {request.RunId} not found.");
        
        run.Status = "Approved";
        run.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}