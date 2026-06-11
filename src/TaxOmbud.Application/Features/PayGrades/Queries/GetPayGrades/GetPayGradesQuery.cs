using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.PayGrades.Queries.GetPayGrades;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetPayGradesQuery() : IRequest<Result<IEnumerable<PayGradeDto>>>;

public record PayGradeDto(
    Guid Id,
    string Name,
    int Level,
    string BasicSalaryBand,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetPayGradesQueryHandler : IRequestHandler<GetPayGradesQuery, Result<IEnumerable<PayGradeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPayGradesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<PayGradeDto>>> Handle(GetPayGradesQuery request, CancellationToken cancellationToken)
    {
        var grades = await _context.PayGrades
            .AsNoTracking()
            .OrderBy(g => g.Level)
            .Select(g => new PayGradeDto(
                g.Id,
                g.Name,
                g.Level,
                g.BasicSalaryBand,
                g.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<PayGradeDto>>.Success(grades);
    }
}
