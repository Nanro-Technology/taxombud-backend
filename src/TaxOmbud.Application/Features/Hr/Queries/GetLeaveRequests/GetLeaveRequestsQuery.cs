using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Hr.Queries.GetLeaveRequests;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetLeaveRequestsQuery(Guid? UserId, string? Status) : IRequest<Result<IEnumerable<LeaveRequestDto>>>;

public record LeaveRequestDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string LeaveType,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Days,
    string Status,
    string? SupervisorNote
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, Result<IEnumerable<LeaveRequestDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.User)
            .AsNoTracking()
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusLower = request.Status.ToLower();
            query = query.Where(l => l.Status == statusLower);
        }

        var items = await query
            .OrderByDescending(l => l.StartDate)
            .Select(l => new LeaveRequestDto(
                l.Id,
                l.UserId,
                l.User.FullName,
                l.LeaveType,
                l.StartDate,
                l.EndDate,
                l.Days,
                l.Status,
                l.SupervisorNote
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<LeaveRequestDto>>.Success(items);
    }
}
