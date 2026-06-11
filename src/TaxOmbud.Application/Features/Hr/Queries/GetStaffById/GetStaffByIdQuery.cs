using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Hr.Queries.GetStaffById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetStaffByIdQuery(Guid Id) : IRequest<Result<StaffDetailDto>>;

public record StaffDetailDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    StaffDepartmentDto? Department,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    DateTimeOffset DateOfBirth,
    string Nationality,
    string MaritalStatus,
    string EmergencyContact,
    string BankAccountNo,
    string BankId,
    string NextOfKin
);

public record StaffDepartmentDto(Guid Id, string Name);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, Result<StaffDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffDetailDto>> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffProfiles
            .Include(s => s.User)
                .ThenInclude(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (staff == null)
            return Result<StaffDetailDto>.NotFound("Staff profile not found.");

        var dto = new StaffDetailDto(
            staff.Id,
            staff.UserId,
            staff.User.FirstName,
            staff.User.LastName,
            staff.User.FullName,
            staff.User.Email,
            staff.User.Phone,
            staff.User.JobTitle,
            staff.User.Department != null ? new StaffDepartmentDto(staff.User.Department.Id, staff.User.Department.Name) : null,
            staff.HireDate,
            staff.EmploymentStatus,
            staff.DateOfBirth,
            staff.Nationality,
            staff.MaritalStatus,
            staff.EmergencyContact,
            staff.BankAccountNo,
            staff.BankId,
            staff.NextOfKin
        );

        return Result<StaffDetailDto>.Success(dto);
    }
}
