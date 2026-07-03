using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Common.Utilities;

namespace TaxOmbud.Application.Complaints.Validators;

public class SubmitComplaintCommandValidator : AbstractValidator<SubmitComplaintCommand>
{
    private static readonly string[] ValidTaxTypes =
        ["PIT", "CIT", "VAT", "CGT", "WHT", "EDT", "PAYE", "Stamp Duty", "Other"];

    private static readonly string[] ValidCategories =
        ["Refund", "Assessment", "Enforcement", "Objection", "Interpretation", "Other"];

    public SubmitComplaintCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
        RuleFor(x => x.TaxType).NotEmpty().Must(t => Array.Exists(ValidTaxTypes, v => v == t))
            .WithMessage("Invalid TaxType specified.");
        RuleFor(x => x.TaxPeriod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ComplaintCategory).NotEmpty().Must(c => Array.Exists(ValidCategories, v => v == c))
            .WithMessage("Invalid ComplaintCategory specified.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
    }
}
