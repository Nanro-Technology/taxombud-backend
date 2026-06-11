using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaints;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? TaxType = null,
    Guid? TaxpayerId = null,
    Guid? AssignedOfficerId = null,
    string? Search = null
) : IRequest<Result<PagedResult<ComplaintSummaryDto>>>;

public record ComplaintSummaryDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string Status,
    string CurrentStage,
    string Priority,
    Guid TaxpayerId,
    string? TaxpayerName,
    Guid? AssignedOfficerId,
    string? AssignedOfficerName,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintsQueryHandler : IRequestHandler<GetComplaintsQuery, Result<PagedResult<ComplaintSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ComplaintSummaryDto>>> Handle(GetComplaintsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Complaints
            .AsNoTracking()
            .Include(c => c.Taxpayer)
            .Include(c => c.AssignedOfficer)
                .ThenInclude(o => o!.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(c => c.Status.ToString() == request.Status);

        if (!string.IsNullOrWhiteSpace(request.TaxType))
            query = query.Where(c => c.TaxType == request.TaxType);

        if (request.TaxpayerId.HasValue)
            query = query.Where(c => c.TaxpayerId == request.TaxpayerId.Value);

        if (request.AssignedOfficerId.HasValue)
            query = query.Where(c => c.AssignedOfficerId == request.AssignedOfficerId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c =>
                c.ReferenceNumber.Contains(search) ||
                c.Subject.ToLower().Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ComplaintSummaryDto(
                c.Id,
                c.ReferenceNumber,
                c.Subject,
                c.TaxType,
                c.TaxPeriod,
                c.ComplaintCategory,
                c.Status.ToString(),
                c.CurrentStage,
                c.Priority,
                c.TaxpayerId,
                c.Taxpayer != null ? c.Taxpayer.FirstName + " " + c.Taxpayer.LastName : null,
                c.AssignedOfficerId,
                c.AssignedOfficer != null && c.AssignedOfficer.User != null
                    ? c.AssignedOfficer.User.FullName : null,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var paged = new PagedResult<ComplaintSummaryDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<ComplaintSummaryDto>>.Success(paged);
    }
}
