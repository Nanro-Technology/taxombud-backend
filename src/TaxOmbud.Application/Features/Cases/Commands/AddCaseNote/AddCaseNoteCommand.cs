using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Features.Cases.Commands.AddCaseNote;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AddCaseNoteCommand(Guid CaseId, string Text, bool IsExternal) : IRequest<Result<AddCaseNoteResponse>>;

public record AddCaseNoteResponse(Guid Id, string NoteText, bool IsExternal, DateTimeOffset CreatedAt);

// ─── Validator ────────────────────────────────────────────────────────────────

public class AddCaseNoteCommandValidator : AbstractValidator<AddCaseNoteCommand>
{
    public AddCaseNoteCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AddCaseNoteCommandHandler : IRequestHandler<AddCaseNoteCommand, Result<AddCaseNoteResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AddCaseNoteCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<AddCaseNoteResponse>> Handle(AddCaseNoteCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (complaint == null)
            return Result<AddCaseNoteResponse>.NotFound("Complaint/Case not found.");

        var authorId = _currentUser.UserId ?? Guid.Empty;
        var note = new ComplaintNote
        {
            Id = Guid.NewGuid(),
            ComplaintId = request.CaseId,
            Body = request.Text,
            Visibility = request.IsExternal ? "external" : "internal",
            AuthorUserId = authorId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.CaseNotes.Add(new CaseNote
        {
            Id = Guid.NewGuid(),
            CaseId = request.CaseId,
            Content = request.Text,
            IsInternal = !request.IsExternal,
            AuthorId = authorId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        _context.Complaints.Entry(complaint).Collection(c => c.Notes).Query().Cast<ComplaintNote>().ToList();
        complaint.Notes.Add(note);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AddCaseNoteResponse>.Success(new AddCaseNoteResponse(
            note.Id,
            note.Body,
            note.Visibility == "external",
            note.CreatedAt
        ));
    }
}
