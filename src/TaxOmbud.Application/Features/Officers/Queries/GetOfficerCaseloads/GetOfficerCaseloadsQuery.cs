using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Officers.Queries.GetOfficerCaseloads;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetOfficerCaseloadsQuery(Guid OfficerId, bool? ActiveOnly) : IRequest<Result<OfficerCaseloadsDto>>;

public record OfficerCaseloadsDto(
    Guid OfficerId,
    IEnumerable<CaseloadDto> Caseloads
);

public record CaseloadDto(
    Guid Id,
    Guid CaseId,
    bool IsActive,
    DateTimeOffset AssignedAt,
    DateTimeOffset? CompletedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetOfficerCaseloadsQueryHandler : IRequestHandler<GetOfficerCaseloadsQuery, Result<OfficerCaseloadsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOfficerCaseloadsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OfficerCaseloadsDto>> Handle(GetOfficerCaseloadsQuery request, CancellationToken cancellationToken)
    {
        var officerExists = await _context.OfficerProfiles.AnyAsync(o => o.Id == request.OfficerId, cancellationToken);
        if (!officerExists)
            return Result<OfficerCaseloadsDto>.NotFound("Officer profile not found.");

        var query = _context.OfficerCaseloads
            .Where(c => c.OfficerProfileId == request.OfficerId)
            .AsNoTracking();

        if (request.ActiveOnly == true)
            query = query.Where(c => c.IsActive);

        var caseloads = await query
            .OrderByDescending(c => c.AssignedAt)
            .Select(c => new CaseloadDto(
                c.Id,
                c.CaseId,
                c.IsActive,
                c.AssignedAt,
                c.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        var dto = new OfficerCaseloadsDto(request.OfficerId, caseloads);
        return Result<OfficerCaseloadsDto>.Success(dto);
    }
}
