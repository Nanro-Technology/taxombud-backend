using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Officers.Queries.GetOfficers;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Officers.Queries.GetAvailableOfficers;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAvailableOfficersQuery(
    Guid? DepartmentId = null,
    string? Specialisation = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<OfficerListDto>>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAvailableOfficersQueryHandler
    : IRequestHandler<GetAvailableOfficersQuery, Result<PagedResult<OfficerListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableOfficersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<OfficerListDto>>> Handle(
        GetAvailableOfficersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OfficerProfiles
            .AsNoTracking()
            .Include(o => o.User)
                .ThenInclude(u => u.Department)
            .Where(o => o.IsAvailable && o.CurrentCaseload < o.MaxCaseload);

        if (request.DepartmentId.HasValue)
            query = query.Where(o => o.User.DepartmentId == request.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(request.Specialisation))
            query = query.Where(o => o.Specialisation == request.Specialisation);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(o => o.CurrentCaseload)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OfficerListDto(
                o.Id,
                o.UserId,
                o.User.FullName,
                o.User.Email,
                o.User.Phone,
                o.User.JobTitle,
                o.User.Department != null
                    ? new OfficerDepartmentDto(o.User.Department.Id, o.User.Department.Name)
                    : null,
                o.MaxCaseload,
                o.CurrentCaseload,
                o.IsAvailable,
                o.EmployeeNumber,
                o.Specialisation,
                o.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OfficerListDto>>.Success(
            new PagedResult<OfficerListDto>(items, totalCount, request.Page, request.PageSize));
    }
}
