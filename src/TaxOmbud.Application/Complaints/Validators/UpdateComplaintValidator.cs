using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Complaints.Validators;

public class UpdateComplaintCommandValidator : AbstractValidator<UpdateComplaintCommand>
{
    public UpdateComplaintCommandValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.TaxType).NotEmpty();
        RuleFor(x => x.TaxPeriod).NotEmpty();
        RuleFor(x => x.ComplaintCategory).NotEmpty();
        RuleFor(x => x.Priority).Must(p => p is "low" or "medium" or "high" or "urgent")
            .WithMessage("Priority must be low, medium, high, or urgent.");
    }
}