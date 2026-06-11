using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Cases.Queries.GetQueue;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetQueueQuery(
    string QueueName,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<QueueResultDto>>;

public record QueueResultDto(
    string Queue,
    IEnumerable<QueueItemDto> Items,
    int Total,
    int Page,
    int PageSize
);

public record QueueItemDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string TaxType,
    string ComplaintCategory,
    string Status,
    string CurrentStage,
    string TaxpayerName,
    string AssignedOfficerName,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetQueueQueryHandler : IRequestHandler<GetQueueQuery, Result<QueueResultDto>>
{
    private readonly IApplicationDbContext _context;

    public GetQueueQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QueueResultDto>> Handle(GetQueueQuery request, CancellationToken cancellationToken)
    {
        var targetStage = request.QueueName.ToLowerInvariant();
        if (targetStage == "intake") targetStage = "input";
        if (targetStage == "verifier") targetStage = "verify";

        var query = _context.Complaints
            .Include(c => c.Taxpayer)
            .Include(c => c.AssignedOfficer!)
                .ThenInclude(o => o.User)
            .Where(c => c.CurrentStage == targetStage)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new QueueItemDto(
                c.Id,
                c.ReferenceNumber,
                c.Subject,
                c.TaxType,
                c.ComplaintCategory,
                c.Status.ToString(),
                c.CurrentStage,
                c.Taxpayer != null ? c.Taxpayer.FirstName + " " + c.Taxpayer.LastName : "Unknown",
                c.AssignedOfficer != null && c.AssignedOfficer.User != null ? c.AssignedOfficer.User.FullName : "Unassigned",
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<QueueResultDto>.Success(new QueueResultDto(request.QueueName, items, total, request.Page, request.PageSize));
    }
}
