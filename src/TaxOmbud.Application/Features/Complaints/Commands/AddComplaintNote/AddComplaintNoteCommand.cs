using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Features.Complaints.Commands.AddComplaintNote;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AddComplaintNoteCommand(
    Guid ComplaintId,
    string Body,
    string Visibility,
    Guid AuthorUserId
) : IRequest<Result<Guid>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class AddComplaintNoteCommandValidator : AbstractValidator<AddComplaintNoteCommand>
{
    public AddComplaintNoteCommandValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Visibility)
            .Must(v => v is "internal" or "external")
            .WithMessage("Visibility must be 'internal' or 'external'.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AddComplaintNoteCommandHandler : IRequestHandler<AddComplaintNoteCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddComplaintNoteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddComplaintNoteCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<Guid>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        var note = new ComplaintNote
        {
            Id = Guid.NewGuid(),
            ComplaintId = request.ComplaintId,
            Body = request.Body,
            Visibility = request.Visibility,
            AuthorUserId = request.AuthorUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.ComplaintNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(note.Id);
    }
}
