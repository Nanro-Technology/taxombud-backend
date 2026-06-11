using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Features.Departments.Queries.GetDepartments;

namespace TaxOmbud.Application.Features.Departments.Queries.GetDepartmentById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetDepartmentByIdQuery(Guid Id) : IRequest<Result<DepartmentDto>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DepartmentDto>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .Include(d => d.HeadUser)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
            return Result<DepartmentDto>.NotFound("Department not found.");

        var dto = new DepartmentDto(
            department.Id,
            department.Name,
            department.RoutingMode,
            department.Description,
            department.HeadUser != null ? new HeadUserDto(department.HeadUser.Id, department.HeadUser.FullName, department.HeadUser.Email) : null
        );

        return Result<DepartmentDto>.Success(dto);
    }
}
