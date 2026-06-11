using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Documents.Commands.CreateDocument;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateDocumentCommand(
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize,
    string EntityType,
    Guid EntityId
) : IRequest<Result<CreatedDocumentResponse>>;

public record CreatedDocumentResponse(
    Guid Id,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FilePath).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FileSize).GreaterThan(0).WithMessage("File size must be greater than zero.");
        RuleFor(x => x.EntityType).NotEmpty()
            .Must(et => Enum.TryParse<DocumentEntityType>(et, true, out _))
            .WithMessage("Invalid entity type.");
        RuleFor(x => x.EntityId).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Result<CreatedDocumentResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreatedDocumentResponse>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DocumentEntityType>(request.EntityType, true, out var entityType))
            return Result<CreatedDocumentResponse>.Failure($"Invalid entity type '{request.EntityType}'.");

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FilePath = request.FilePath,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            EntityType = entityType,
            EntityId = request.EntityId
        };

        _context.Documents.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreatedDocumentResponse(doc.Id, doc.FileName, doc.FilePath, doc.ContentType, doc.FileSize);
        return Result<CreatedDocumentResponse>.Success(response);
    }
}
