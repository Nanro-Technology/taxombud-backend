using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Complaints.Validators;

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