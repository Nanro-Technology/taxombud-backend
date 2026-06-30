using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Complaints.Validators;

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