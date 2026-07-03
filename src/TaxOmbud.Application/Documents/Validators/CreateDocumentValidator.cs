using System;
using FluentValidation;
using TaxOmbud.Application.Documents.DTOs;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Documents.Validators;

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
