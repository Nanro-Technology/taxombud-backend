using System;
using FluentValidation;
using TaxOmbud.Application.Documents.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Documents.Validators;

public class AddDocumentVersionCommandValidator : AbstractValidator<AddDocumentVersionCommand>
{
    public AddDocumentVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.FilePath).NotEmpty().MaximumLength(1000);
    }
}