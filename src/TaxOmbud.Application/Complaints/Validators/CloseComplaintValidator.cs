using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Complaints.Validators;

public class CloseComplaintCommandValidator : AbstractValidator<CloseComplaintCommand>
{
    public CloseComplaintCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10);
    }
}
