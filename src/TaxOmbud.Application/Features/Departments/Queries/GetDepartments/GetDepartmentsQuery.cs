using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Departments.Queries.GetDepartments;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDepartmentsQuery() : IRequest<Result<IEnumerable<DepartmentDto>>>;

public record DepartmentDto(
    Guid Id,
    string Name,
    string RoutingMode,
    string? Description,
    HeadUserDto? HeadUser
);

public record HeadUserDto(Guid Id, string FullName, string Email);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, Result<IEnumerable<DepartmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Include(d => d.HeadUser)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.RoutingMode,
                d.Description,
                d.HeadUser != null ? new HeadUserDto(d.HeadUser.Id, d.HeadUser.FullName, d.HeadUser.Email) : null
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<DepartmentDto>>.Success(departments);
    }
}
