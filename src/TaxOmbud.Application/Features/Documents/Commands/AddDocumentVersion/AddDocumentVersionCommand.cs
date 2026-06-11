using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Features.Documents.Commands.AddDocumentVersion;

// ─── Command ─────────────────────────────────────────────────────────────────

public record AddDocumentVersionCommand(Guid DocumentId, string FilePath) : IRequest<Result<AddedVersionResponse>>;

public record AddedVersionResponse(
    Guid Id,
    int VersionNumber,
    string FilePath
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class AddDocumentVersionCommandValidator : AbstractValidator<AddDocumentVersionCommand>
{
    public AddDocumentVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.FilePath).NotEmpty().MaximumLength(1000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class AddDocumentVersionCommandHandler : IRequestHandler<AddDocumentVersionCommand, Result<AddedVersionResponse>>
{
    private readonly IApplicationDbContext _context;

    public AddDocumentVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AddedVersionResponse>> Handle(AddDocumentVersionCommand request, CancellationToken cancellationToken)
    {
        var doc = await _context.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (doc == null)
            return Result<AddedVersionResponse>.NotFound("Document not found.");

        var nextVersion = doc.Versions.Count + 1;
        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            VersionNumber = nextVersion,
            FilePath = request.FilePath
        };

        doc.Versions.Add(version);
        doc.FilePath = request.FilePath;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new AddedVersionResponse(version.Id, version.VersionNumber, version.FilePath);
        return Result<AddedVersionResponse>.Success(response);
    }
}
