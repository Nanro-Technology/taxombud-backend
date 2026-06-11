using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.PayGrades.Queries.GetPayGradeById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetPayGradeByIdQuery(Guid Id) : IRequest<Result<PayGradeDetailDto>>;

public record PayGradeDetailDto(
    Guid Id,
    string Name,
    int Level,
    string BasicSalaryBand,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetPayGradeByIdQueryHandler : IRequestHandler<GetPayGradeByIdQuery, Result<PayGradeDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPayGradeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PayGradeDetailDto>> Handle(GetPayGradeByIdQuery request, CancellationToken cancellationToken)
    {
        var grade = await _context.PayGrades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade == null)
            return Result<PayGradeDetailDto>.NotFound("Pay grade not found.");

        var dto = new PayGradeDetailDto(
            grade.Id,
            grade.Name,
            grade.Level,
            grade.BasicSalaryBand,
            grade.CreatedAt
        );

        return Result<PayGradeDetailDto>.Success(dto);
    }
}
