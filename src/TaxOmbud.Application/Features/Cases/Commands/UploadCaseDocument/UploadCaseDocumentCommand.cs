using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.UploadCaseDocument;

public record UploadCaseDocumentCommand(
    Guid CaseId,
    IFormFile File
) : IRequest<Result<Guid>>;

public class UploadCaseDocumentCommandValidator
    : AbstractValidator<UploadCaseDocumentCommand>
{
    private static readonly string[] AllowedTypes =
        ["application/pdf", "image/jpeg", "image/png", "application/msword",
         "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    public UploadCaseDocumentCommandValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("A file must be provided.");
        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("File must not exceed 10 MB.");
        RuleFor(x => x.File.ContentType)
            .Must(ct => AllowedTypes.Contains(ct))
            .WithMessage("Unsupported file type. Allowed: PDF, JPEG, PNG, DOC, DOCX.");
    }
}

public class UploadCaseDocumentCommandHandler
    : IRequestHandler<UploadCaseDocumentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    public UploadCaseDocumentCommandHandler(
        IApplicationDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result<Guid>> Handle(
        UploadCaseDocumentCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Cases
            .AnyAsync(c => c.Id == request.CaseId, cancellationToken);

        if (!exists)
            return Result<Guid>.NotFound($"Case '{request.CaseId}' was not found.");

        await using var stream = request.File.OpenReadStream();
        var path = await _storage.StoreAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            cancellationToken);

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.File.FileName,
            FilePath = path,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            EntityType = DocumentEntityType.Case,
            EntityId = request.CaseId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Documents.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(doc.Id);
    }
}
