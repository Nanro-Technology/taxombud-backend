using System;
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

namespace TaxOmbud.Application.Features.Complaints.Commands.UploadComplaintDocument;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UploadComplaintDocumentCommand(
    Guid ComplaintId,
    IFormFile File
) : IRequest<Result<Guid>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class UploadComplaintDocumentCommandValidator
    : AbstractValidator<UploadComplaintDocumentCommand>
{
    private static readonly string[] AllowedTypes =
        ["application/pdf", "image/jpeg", "image/png", "application/msword",
         "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    public UploadComplaintDocumentCommandValidator()
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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UploadComplaintDocumentCommandHandler
    : IRequestHandler<UploadComplaintDocumentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    public UploadComplaintDocumentCommandHandler(
        IApplicationDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<Result<Guid>> Handle(
        UploadComplaintDocumentCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Complaints
            .AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (!exists)
            return Result<Guid>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

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
            EntityType = DocumentEntityType.Complaint,
            EntityId = request.ComplaintId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Documents.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(doc.Id);
    }
}
