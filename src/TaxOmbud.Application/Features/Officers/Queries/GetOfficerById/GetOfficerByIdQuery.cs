using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Officers.Queries.GetOfficerById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetOfficerByIdQuery(Guid Id) : IRequest<Result<OfficerDetailDto>>;

public record OfficerDetailDto(
    Guid Id,
    Guid UserId,
    string? FullName,
    string? Email,
    string? Phone,
    string? JobTitle,
    OfficerDepartmentDto? Department,
    int MaxCaseload,
    int CurrentCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation,
    int ActiveCaseloads,
    DateTimeOffset CreatedAt
);

public record OfficerDepartmentDto(Guid Id, string Name);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetOfficerByIdQueryHandler : IRequestHandler<GetOfficerByIdQuery, Result<OfficerDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOfficerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OfficerDetailDto>> Handle(GetOfficerByIdQuery request, CancellationToken cancellationToken)
    {
        var officer = await _context.OfficerProfiles
            .Include(o => o.User)
                .ThenInclude(u => u!.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (officer == null)
            return Result<OfficerDetailDto>.NotFound("Officer profile not found.");

        var activeCaseloads = await _context.OfficerCaseloads
            .CountAsync(c => c.OfficerProfileId == request.Id && c.IsActive, cancellationToken);

        var dto = new OfficerDetailDto(
            officer.Id,
            officer.UserId,
            officer.User?.FullName,
            officer.User?.Email,
            officer.User?.Phone,
            officer.User?.JobTitle,
            officer.User?.Department != null
                ? new OfficerDepartmentDto(officer.User.Department.Id, officer.User.Department.Name)
                : null,
            officer.MaxCaseload,
            officer.CurrentCaseload,
            officer.IsAvailable,
            officer.EmployeeNumber,
            officer.Specialisation,
            activeCaseloads,
            officer.CreatedAt
        );

        return Result<OfficerDetailDto>.Success(dto);
    }
}
