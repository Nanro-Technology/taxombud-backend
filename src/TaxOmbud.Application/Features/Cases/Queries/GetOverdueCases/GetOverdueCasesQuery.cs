using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Application.Features.Cases.Queries.GetCases;

namespace TaxOmbud.Application.Features.Cases.Queries.GetOverdueCases;

public record GetOverdueCasesQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<TaxOmbud.Application.Common.Models.Result<PagedResult<CaseListDto>>>;

public class GetOverdueCasesQueryHandler : IRequestHandler<GetOverdueCasesQuery, TaxOmbud.Application.Common.Models.Result<PagedResult<CaseListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOverdueCasesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaxOmbud.Application.Common.Models.Result<PagedResult<CaseListDto>>> Handle(GetOverdueCasesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var query = _context.Cases
            .Include(c => c.Complaint)
            .Include(c => c.AssignedOfficer)
            .Where(c => c.Status != CaseStatus.Closed && c.DueDate.HasValue && c.DueDate.Value < now)
            .AsNoTracking()
            .OrderBy(c => c.DueDate);

        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Adapt<global::System.Collections.Generic.List<CaseListDto>>();

        return TaxOmbud.Application.Common.Models.Result<PagedResult<CaseListDto>>.Success(new PagedResult<CaseListDto>(dtos, total, request.Page, request.PageSize));
    }
}
