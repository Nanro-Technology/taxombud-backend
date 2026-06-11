using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Features.Hr.Queries.GetStaff;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetStaffQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<StaffListDto>>>;

public record StaffListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    string DepartmentName,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    string MaritalStatus
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetStaffQueryHandler : IRequestHandler<GetStaffQuery, Result<PagedResult<StaffListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<StaffListDto>>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(s =>
                s.User.FirstName.ToLower().Contains(searchLower) ||
                s.User.LastName.ToLower().Contains(searchLower) ||
                s.User.Email.Contains(searchLower));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.HireDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StaffListDto(
                s.Id,
                s.UserId,
                s.User.FullName,
                s.User.Email,
                s.User.Phone,
                s.User.JobTitle,
                s.User.Department != null ? s.User.Department.Name : "Unassigned",
                s.HireDate,
                s.EmploymentStatus,
                s.MaritalStatus
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<StaffListDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<StaffListDto>>.Success(pagedResult);
    }
}
