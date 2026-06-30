using System;
using FluentValidation;
using TaxOmbud.Application.Complaints.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Complaints.Validators;

public class UpdateComplaintStatusCommandValidator : AbstractValidator<UpdateComplaintStatusCommand>
{
    public UpdateComplaintStatusCommandValidator()
    {
        RuleFor(x => x.ComplaintId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}