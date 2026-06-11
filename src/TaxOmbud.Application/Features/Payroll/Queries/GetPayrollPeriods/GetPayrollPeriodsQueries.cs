using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Payroll.Queries.GetPayrollPeriods;

public record GetPayrollPeriodsQueries : IRequest<Result<GetPayrollPeriodsResponse>>
{
}

public class GetPayrollPeriodsResponse
{
    public bool Success { get; set; }
}

public class GetPayrollPeriodsQueriesHandler : IRequestHandler<GetPayrollPeriodsQueries, Result<GetPayrollPeriodsResponse>>
{
    public async Task<Result<GetPayrollPeriodsResponse>> Handle(GetPayrollPeriodsQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; return Result<GetPayrollPeriodsResponse>.Success(new GetPayrollPeriodsResponse { Success = true });
    }
}
