using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Appeals.Queries.GetAppealById;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetAppealByIdQuery(Guid Id) : IRequest<Result<AppealDetailDto>>;

public record AppealDetailDto(
    Guid Id,
    Guid CaseId,
    string CaseNumber,
    string CaseSubject,
    string Reason,
    string Status,
    Guid? ReviewedByUserId,
    string? ReviewNote,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    IEnumerable<AppealStatusHistoryDto> StatusHistory
);

public record AppealStatusHistoryDto(
    Guid Id,
    string PreviousStatus,
    string NewStatus,
    string? Notes,
    Guid ChangedByUserId,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetAppealByIdQueryHandler : IRequestHandler<GetAppealByIdQuery, Result<AppealDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppealByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppealDetailDto>> Handle(GetAppealByIdQuery request, CancellationToken cancellationToken)
    {
        var appeal = await _context.Appeals
            .Include(a => a.Case)
            .Include(a => a.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (appeal == null)
            return Result<AppealDetailDto>.NotFound("Appeal not found.");

        var dto = new AppealDetailDto(
            appeal.Id,
            appeal.CaseId,
            appeal.Case!.CaseNumber.Value,
            appeal.Case.Subject,
            appeal.Reason,
            appeal.Status.ToString(),
            appeal.ReviewedByUserId,
            appeal.ReviewNote,
            appeal.ReviewedAt,
            appeal.CreatedAt,
            appeal.StatusHistory.Select(h => new AppealStatusHistoryDto(
                h.Id,
                h.OldStatus.ToString(),
                h.NewStatus.ToString(),
                h.Reason,
                h.ChangedByUserId,
                h.TransitionedAt
            ))
        );

        return Result<AppealDetailDto>.Success(dto);
    }
}
