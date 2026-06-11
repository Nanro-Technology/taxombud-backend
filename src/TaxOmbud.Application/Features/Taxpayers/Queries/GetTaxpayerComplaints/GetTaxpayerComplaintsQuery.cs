using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Complaints.Queries.GetComplaints;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayerComplaints;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetTaxpayerComplaintsQuery(
    Guid TaxpayerId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ComplaintSummaryDto>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetTaxpayerComplaintsQueryHandler
    : IRequestHandler<GetTaxpayerComplaintsQuery, Result<PagedResult<ComplaintSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxpayerComplaintsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ComplaintSummaryDto>>> Handle(
        GetTaxpayerComplaintsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Complaints
            .AsNoTracking()
            .Include(c => c.Taxpayer)
            .Include(c => c.AssignedOfficer)
                .ThenInclude(o => o!.User)
            .Where(c => c.TaxpayerId == request.TaxpayerId);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(c => c.Status.ToString() == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ComplaintSummaryDto(
                c.Id,
                c.ReferenceNumber,
                c.Subject,
                c.TaxType.ToString(),
                c.TaxPeriod,
                c.ComplaintCategory.ToString(),
                c.Status.ToString(),
                c.CurrentStage.ToString(),
                c.Priority.ToString(),
                c.TaxpayerId,
                c.Taxpayer != null ? $"{c.Taxpayer.FirstName} {c.Taxpayer.LastName}" : null,
                c.AssignedOfficerId,
                c.AssignedOfficer != null ? c.AssignedOfficer.User!.FullName : null,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ComplaintSummaryDto>>.Success(
            new PagedResult<ComplaintSummaryDto>(items, totalCount, request.Page, request.PageSize));
    }
}
