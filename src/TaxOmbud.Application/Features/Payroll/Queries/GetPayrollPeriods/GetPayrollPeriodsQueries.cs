using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetPayrollPeriods;

public record GetPayrollPeriodsQueries : IRequest<Result<List<PayrollPeriod>>> { }

public class GetPayrollPeriodsQueriesHandler : IRequestHandler<GetPayrollPeriodsQueries, Result<List<PayrollPeriod>>>
{
    private readonly IApplicationDbContext _context;
    public GetPayrollPeriodsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<PayrollPeriod>>> Handle(GetPayrollPeriodsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.PayrollPeriods.ToListAsync(cancellationToken);
        return Result<List<PayrollPeriod>>.Success(list);
    }
}