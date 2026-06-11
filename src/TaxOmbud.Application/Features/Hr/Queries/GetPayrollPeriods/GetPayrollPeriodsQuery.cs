using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Queries.GetPayrollPeriods;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetPayrollPeriodsQuery() : IRequest<Result<IEnumerable<PayrollPeriod>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetPayrollPeriodsQueryHandler : IRequestHandler<GetPayrollPeriodsQuery, Result<IEnumerable<PayrollPeriod>>>
{
    private readonly IApplicationDbContext _context;

    public GetPayrollPeriodsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<PayrollPeriod>>> Handle(GetPayrollPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _context.PayrollPeriods.AsNoTracking().ToListAsync(cancellationToken);
        return Result<IEnumerable<PayrollPeriod>>.Success(periods);
    }
}
