using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Queries.GetInvoices;

public record GetInvoicesQueries : IRequest<Result<GetInvoicesResponse>>
{
}

public class GetInvoicesResponse
{
    public bool Success { get; set; }
}

public class GetInvoicesQueriesHandler : IRequestHandler<GetInvoicesQueries, Result<GetInvoicesResponse>>
{
    public async Task<Result<GetInvoicesResponse>> Handle(GetInvoicesQueries request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GetInvoicesResponse>.Success(new GetInvoicesResponse { Success = true });
    }
}