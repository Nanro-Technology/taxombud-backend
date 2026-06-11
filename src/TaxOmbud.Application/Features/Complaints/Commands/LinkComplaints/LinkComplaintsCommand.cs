using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Features.Complaints.Commands.LinkComplaints;

// ─── Command ─────────────────────────────────────────────────────────────────

public record LinkComplaintsCommand(
    Guid SourceComplaintId,
    Guid TargetComplaintId,
    string? LinkType
) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class LinkComplaintsCommandValidator : AbstractValidator<LinkComplaintsCommand>
{
    public LinkComplaintsCommandValidator()
    {
        RuleFor(x => x.SourceComplaintId).NotEmpty();
        RuleFor(x => x.TargetComplaintId).NotEmpty();
        RuleFor(x => x).Must(x => x.SourceComplaintId != x.TargetComplaintId)
            .WithMessage("A complaint cannot be linked to itself.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class LinkComplaintsCommandHandler : IRequestHandler<LinkComplaintsCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public LinkComplaintsCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<object?>> Handle(LinkComplaintsCommand request, CancellationToken cancellationToken)
    {
        var sourceExists = await _context.Complaints
            .AnyAsync(c => c.Id == request.SourceComplaintId, cancellationToken);
        if (!sourceExists)
            return Result<object?>.NotFound($"Source complaint '{request.SourceComplaintId}' was not found.");

        var targetExists = await _context.Complaints
            .AnyAsync(c => c.Id == request.TargetComplaintId, cancellationToken);
        if (!targetExists)
            return Result<object?>.NotFound($"Target complaint '{request.TargetComplaintId}' was not found.");

        // Check if link already exists in either direction
        var linkExists = await _context.ComplaintLinks
            .AnyAsync(l => 
                (l.SourceComplaintId == request.SourceComplaintId && l.TargetComplaintId == request.TargetComplaintId) ||
                (l.SourceComplaintId == request.TargetComplaintId && l.TargetComplaintId == request.SourceComplaintId),
                cancellationToken);

        if (linkExists)
            return Result<object?>.Conflict("A link already exists between these complaints.");

        var link = new ComplaintLink
        {
            Id = Guid.NewGuid(),
            SourceComplaintId = request.SourceComplaintId,
            TargetComplaintId = request.TargetComplaintId,
            LinkType = string.IsNullOrWhiteSpace(request.LinkType) ? "related" : request.LinkType.Trim(),
            LinkedByUserId = _currentUser.UserId ?? Guid.Empty
        };

        _context.ComplaintLinks.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
