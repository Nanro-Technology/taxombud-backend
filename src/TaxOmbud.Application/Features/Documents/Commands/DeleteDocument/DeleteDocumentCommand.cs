using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Documents.Commands.DeleteDocument;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeleteDocumentCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    public DeleteDocumentCommandHandler(IApplicationDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result<Unit>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc == null)
            return Result<Unit>.NotFound("Document not found.");

        await _storage.DeleteAsync(doc.FilePath, cancellationToken);
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
