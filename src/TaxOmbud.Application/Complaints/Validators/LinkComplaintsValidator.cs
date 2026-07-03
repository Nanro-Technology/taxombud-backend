using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Complaints;

namespace TaxOmbud.Application.Complaints.Validators;

public class LinkComplaintsCommandValidator : AbstractValidator<LinkComplaintsCommand>
{
    public LinkComplaintsCommandValidator()
    {
        RuleFor(x => x.SourceComplaintId).NotEmpty();
        RuleFor(x => x.TargetComplaintId).NotEmpty();
        RuleFor(x => x).Must(x => x.SourceComplaintId != x.TargetComplaintId)
            .WithMessage("A complaint cannot be linked to itself.");
    }
}
