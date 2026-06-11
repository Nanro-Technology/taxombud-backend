using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Appeals.Queries.GetAppeals;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAppealsQuery(
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<AppealListDto>>>;

public record AppealListDto(
    Guid Id,
    Guid CaseId,
    string CaseNumber,
    string CaseSubject,
    string Reason,
    string Status,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAppealsQueryHandler : IRequestHandler<GetAppealsQuery, Result<PagedResult<AppealListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppealsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AppealListDto>>> Handle(GetAppealsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appeals
            .Include(a => a.Case)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppealStatus>(request.Status, true, out var appealStatus))
        {
            query = query.Where(a => a.Status == appealStatus);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppealListDto(
                a.Id,
                a.CaseId,
                a.Case!.CaseNumber.Value,
                a.Case.Subject,
                a.Reason,
                a.Status.ToString(),
                a.ReviewedByUserId,
                a.ReviewedAt,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AppealListDto>>.Success(new PagedResult<AppealListDto>(items, total, request.Page, request.PageSize));
    }
}
