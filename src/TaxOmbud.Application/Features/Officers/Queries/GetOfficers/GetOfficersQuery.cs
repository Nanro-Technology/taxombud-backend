using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Officers.Queries.GetOfficers;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetOfficersQuery(
    Guid? DepartmentId,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<OfficerListDto>>>;

public record OfficerListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    OfficerDepartmentDto? Department,
    int MaxCaseload,
    int CurrentCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation,
    DateTimeOffset CreatedAt
);

public record OfficerDepartmentDto(Guid Id, string Name);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetOfficersQueryHandler : IRequestHandler<GetOfficersQuery, Result<PagedResult<OfficerListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOfficersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<OfficerListDto>>> Handle(GetOfficersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OfficerProfiles
            .Include(o => o.User)
                .ThenInclude(u => u!.Department)
            .AsNoTracking()
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            query = query.Where(o => o.User != null && o.User.DepartmentId == request.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var lower = request.Search.ToLower();
            query = query.Where(o => o.User != null && (
                o.User.FirstName.ToLower().Contains(lower) ||
                o.User.LastName.ToLower().Contains(lower) ||
                o.User.Email.Contains(lower)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(o => o.User!.LastName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OfficerListDto(
                o.Id,
                o.UserId,
                o.User != null ? o.User.FullName : "Unknown",
                o.User != null ? o.User.Email : "",
                o.User != null ? o.User.Phone : null,
                o.User != null ? o.User.JobTitle : null,
                o.User != null && o.User.Department != null
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

        var pagedResult = new PagedResult<OfficerListDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<OfficerListDto>>.Success(pagedResult);
    }
}
