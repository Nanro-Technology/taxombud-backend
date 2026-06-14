using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetLoanRequests;

public record GetLoanRequestsQueries : IRequest<Result<List<LoanRequest>>> { }

public class GetLoanRequestsQueriesHandler : IRequestHandler<GetLoanRequestsQueries, Result<List<LoanRequest>>>
{
    private readonly IApplicationDbContext _context;
    public GetLoanRequestsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<LoanRequest>>> Handle(GetLoanRequestsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.LoanRequests.ToListAsync(cancellationToken);
        return Result<List<LoanRequest>>.Success(list);
    }
}