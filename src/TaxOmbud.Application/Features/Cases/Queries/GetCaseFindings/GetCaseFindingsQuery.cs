using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Cases.Queries.GetCaseFindings;

public record GetCaseFindingsQuery(Guid CaseId) : IRequest<Result<IReadOnlyList<CaseFindingDto>>>;

public record CaseFindingDto(
    Guid Id,
    Guid CaseId,
    string Description,
    DateTimeOffset CreatedAt,
    Guid? CreatedBy
);

public class GetCaseFindingsQueryHandler : IRequestHandler<GetCaseFindingsQuery, Result<IReadOnlyList<CaseFindingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCaseFindingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CaseFindingDto>>> Handle(
        GetCaseFindingsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Cases.AnyAsync(c => c.Id == request.CaseId, cancellationToken);
        if (!exists)
            return Result<IReadOnlyList<CaseFindingDto>>.NotFound($"Case '{request.CaseId}' was not found.");

        var findings = await _context.CaseFindings
            .AsNoTracking()
            .Where(f => f.CaseId == request.CaseId)
            .OrderBy(f => f.CreatedAt)
            .Select(f => new CaseFindingDto(
                f.Id,
                f.CaseId,
                f.Description,
                f.CreatedAt,
                f.CreatedBy
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CaseFindingDto>>.Success(findings.AsReadOnly());
    }
}
