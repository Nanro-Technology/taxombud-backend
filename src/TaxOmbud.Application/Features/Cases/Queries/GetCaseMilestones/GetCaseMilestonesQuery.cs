using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Cases.Queries.GetCaseMilestones;

public record GetCaseMilestonesQuery(Guid CaseId) : IRequest<Result<IReadOnlyList<CaseMilestoneDto>>>;

public record CaseMilestoneDto(
    Guid Id,
    Guid CaseId,
    string Title,
    string? Description,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedAt,
    bool IsCompleted
);

public class GetCaseMilestonesQueryHandler : IRequestHandler<GetCaseMilestonesQuery, Result<IReadOnlyList<CaseMilestoneDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseMilestonesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CaseMilestoneDto>>> Handle(
        GetCaseMilestonesQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Cases.AnyAsync(c => c.Id == request.CaseId, cancellationToken);
        if (!exists)
            return Result<IReadOnlyList<CaseMilestoneDto>>.NotFound($"Case '{request.CaseId}' was not found.");

        var milestones = await _context.CaseMilestones
            .AsNoTracking()
            .Where(m => m.CaseId == request.CaseId)
            .OrderBy(m => m.TargetDate)
            .Select(m => new CaseMilestoneDto(
                m.Id,
                m.CaseId,
                m.Title,
                m.Description,
                m.TargetDate,
                m.CompletedAt,
                m.IsCompleted
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CaseMilestoneDto>>.Success(milestones.AsReadOnly());
    }
}
