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

namespace TaxOmbud.Application.Features.Users.Queries.GetUsers;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetUsersQuery(
    string? Search,
    string? Status,
    Guid? DepartmentId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<UserListDto>>>;

public record UserListDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    DepartmentDto? Department,
    string Status,
    bool CanSignIn,
    IEnumerable<RoleDto> Roles
);

public record DepartmentDto(Guid Id, string Name);
public record RoleDto(Guid Id, string Name, string Code);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<UserListDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UserStatus>(request.Status, true, out var userStatus))
        {
            query = query.Where(u => u.Status == userStatus);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.Email,
                u.Phone,
                u.JobTitle,
                u.EmploymentType,
                u.Department != null ? new DepartmentDto(u.Department.Id, u.Department.Name) : null,
                u.Status.ToString(),
                u.CanSignIn,
                u.UserRoles.Select(ur => new RoleDto(ur.Role!.Id, ur.Role.Name, ur.Role.Code))
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<UserListDto>(items, total, request.Page, request.PageSize);
        return Result<PagedResult<UserListDto>>.Success(pagedResult);
    }
}
