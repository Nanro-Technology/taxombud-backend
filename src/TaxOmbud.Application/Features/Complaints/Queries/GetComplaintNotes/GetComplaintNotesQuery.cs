using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Queries.GetComplaintNotes;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetComplaintNotesQuery(Guid ComplaintId) : IRequest<Result<IReadOnlyList<ComplaintNoteDto>>>;

public record ComplaintNoteDto(
    Guid Id,
    string Body,
    string Visibility,
    Guid AuthorUserId,
    DateTimeOffset CreatedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetComplaintNotesQueryHandler
    : IRequestHandler<GetComplaintNotesQuery, Result<IReadOnlyList<ComplaintNoteDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintNotesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ComplaintNoteDto>>> Handle(
        GetComplaintNotesQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<ComplaintNoteDto>>.NotFound(
                $"Complaint '{request.ComplaintId}' was not found.");

        var notes = await _context.ComplaintNotes
            .AsNoTracking()
            .Where(n => n.ComplaintId == request.ComplaintId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new ComplaintNoteDto(n.Id, n.Body, n.Visibility, n.AuthorUserId, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ComplaintNoteDto>>.Success(notes.AsReadOnly());
    }
}
