using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetRelatedComplaints;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetRelatedComplaintsQuery(Guid ComplaintId) : IRequest<Result<IReadOnlyList<RelatedComplaintDto>>>;

public record RelatedComplaintDto(
    Guid LinkId,
    Guid ComplaintId,
    string ReferenceNumber,
    string Subject,
    string Status,
    string LinkType
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetRelatedComplaintsQueryHandler : IRequestHandler<GetRelatedComplaintsQuery, Result<IReadOnlyList<RelatedComplaintDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRelatedComplaintsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<RelatedComplaintDto>>> Handle(
        GetRelatedComplaintsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<RelatedComplaintDto>>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        var links = await _context.ComplaintLinks
            .AsNoTracking()
            .Include(l => l.SourceComplaint)
            .Include(l => l.TargetComplaint)
            .Where(l => l.SourceComplaintId == request.ComplaintId || l.TargetComplaintId == request.ComplaintId)
            .ToListAsync(cancellationToken);

        var dtos = links.Select(l =>
        {
            var isSource = l.SourceComplaintId == request.ComplaintId;
            var related = isSource ? l.TargetComplaint : l.SourceComplaint;

            return new RelatedComplaintDto(
                LinkId: l.Id,
                ComplaintId: related.Id,
                ReferenceNumber: related.ReferenceNumber,
                Subject: related.Subject,
                Status: related.Status.ToString(),
                LinkType: l.LinkType
            );
        }).ToList();

        return Result<IReadOnlyList<RelatedComplaintDto>>.Success(dtos.AsReadOnly());
    }
}
