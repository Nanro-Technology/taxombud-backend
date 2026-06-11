using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Queries.GetLeaveRequests;

public record GetLeaveRequestsQueries : IRequest<Result<List<LeaveRequest>>> { }

public class GetLeaveRequestsQueriesHandler : IRequestHandler<GetLeaveRequestsQueries, Result<List<LeaveRequest>>>
{
    private readonly IApplicationDbContext _context;
    public GetLeaveRequestsQueriesHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<LeaveRequest>>> Handle(GetLeaveRequestsQueries request, CancellationToken cancellationToken)
    {
        var list = await _context.LeaveRequests.ToListAsync(cancellationToken);
        return Result<List<LeaveRequest>>.Success(list);
    }
}