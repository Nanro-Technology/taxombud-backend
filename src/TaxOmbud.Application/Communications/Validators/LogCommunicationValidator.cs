using System;
using FluentValidation;
using TaxOmbud.Application.Communications.DTOs;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Communications.Validators;

public class LogCommunicationCommandValidator : AbstractValidator<LogCommunicationCommand>
{
    public LogCommunicationCommandValidator()
    {
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Recipient).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientName).MaximumLength(200);
        RuleFor(x => x.RelatedEntityType).MaximumLength(100);
    }
}