using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaintTimeline;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintTimelineQuery(Guid ComplaintId) : IRequest<Result<IReadOnlyList<TimelineEventDto>>>;

public record TimelineEventDto(
    string EventType,
    string Description,
    string? OldStatus,
    string? NewStatus,
    string? ChangedBy,
    DateTimeOffset OccurredAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintTimelineQueryHandler
    : IRequestHandler<GetComplaintTimelineQuery, Result<IReadOnlyList<TimelineEventDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintTimelineQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<TimelineEventDto>>> Handle(
        GetComplaintTimelineQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<TimelineEventDto>>.NotFound(
                $"Complaint '{request.ComplaintId}' was not found.");

        var history = await _context.ComplaintStatusHistory
            .AsNoTracking()
            .Where(h => h.ComplaintId == request.ComplaintId)
            .OrderBy(h => h.TransitionedAt)
            .ToListAsync(cancellationToken);

        var timeline = history.Select(h => new TimelineEventDto(
            EventType: "StatusChange",
            Description: $"Status changed from {h.OldStatus} to {h.NewStatus}",
            OldStatus: h.OldStatus.ToString(),
            NewStatus: h.NewStatus.ToString(),
            ChangedBy: h.ChangedByUserId.ToString(),
            OccurredAt: h.TransitionedAt
        )).ToList();

        return Result<IReadOnlyList<TimelineEventDto>>.Success(timeline.AsReadOnly());
    }
}
