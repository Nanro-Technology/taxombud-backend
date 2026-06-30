using System;
using FluentValidation;
using TaxOmbud.Application.Webhooks.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Webhooks.Validators;

public class UpdateWebhookCommandValidator : AbstractValidator<UpdateWebhookCommand>
{
    public UpdateWebhookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format.");
        RuleFor(x => x.EventTypes)
            .NotEmpty()
            .WithMessage("At least one event type is required.");
    }
}