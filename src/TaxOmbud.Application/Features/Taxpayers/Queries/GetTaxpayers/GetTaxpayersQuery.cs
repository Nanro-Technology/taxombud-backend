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

namespace TaxOmbud.Application.Features.Taxpayers.Queries.GetTaxpayers;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetTaxpayersQuery(
    string? Search,
    string? Type,
    bool? IsVerified,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<TaxpayerListDto>>>;

public record TaxpayerListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string TaxpayerType,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? CompanyName,
    string? RcNumber,
    bool IsVerified,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetTaxpayersQueryHandler : IRequestHandler<GetTaxpayersQuery, Result<PagedResult<TaxpayerListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxpayersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<TaxpayerListDto>>> Handle(GetTaxpayersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TaxpayerProfiles
            .Include(t => t.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(t =>
                (t.User != null && t.User.FirstName.ToLower().Contains(searchLower)) ||
                (t.User != null && t.User.LastName.ToLower().Contains(searchLower)) ||
                (t.User != null && t.User.Email.Contains(searchLower)) ||
                (t.CompanyName != null && t.CompanyName.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TaxpayerType>(request.Type, true, out var taxpayerType))
        {
            query = query.Where(t => t.TaxpayerType == taxpayerType);
        }

        if (request.IsVerified.HasValue)
        {
            query = query.Where(t => t.IsVerified == request.IsVerified.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TaxpayerListDto(
                t.Id,
                t.UserId,
                t.User != null ? t.User.FullName : "Unknown",
                t.User != null ? t.User.Email : "",
                t.User != null ? t.User.Phone : null,
                t.TaxpayerType.ToString(),
                t.TinNumber,
                t.Nin,
                t.Bvn,
                t.CompanyName,
                t.RcNumber,
                t.IsVerified,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<TaxpayerListDto>>.Success(new PagedResult<TaxpayerListDto>(items, total, request.Page, request.PageSize));
    }
}
