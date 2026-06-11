using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaints;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetMyComplaints;

public record GetMyComplaintsQuery(
    string? Search,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ComplaintSummaryDto>>>;

public class GetMyComplaintsQueryHandler : IRequestHandler<GetMyComplaintsQuery, Result<PagedResult<ComplaintSummaryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyComplaintsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ComplaintSummaryDto>>> Handle(GetMyComplaintsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId is null)
        {
            return Result<PagedResult<ComplaintSummaryDto>>.Failure("User is not authenticated.");
        }
        var query = _context.Complaints
            .Include(c => c.Taxpayer)
            .Where(c => c.TaxpayerId == currentUserId.Value)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.ReferenceNumber.Contains(request.Search) || 
                                     c.Description.Contains(request.Search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && global::System.Enum.TryParse<Domain.Enums.ComplaintStatus>(request.Status, true, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Adapt<global::System.Collections.Generic.List<ComplaintSummaryDto>>();

        return Result<PagedResult<ComplaintSummaryDto>>.Success(new PagedResult<ComplaintSummaryDto>(dtos, total, request.Page, request.PageSize));
    }
}
