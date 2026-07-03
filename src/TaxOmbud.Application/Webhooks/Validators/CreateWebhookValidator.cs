using FluentValidation;
using TaxOmbud.Application.Webhooks.DTOs;

namespace TaxOmbud.Application.Webhooks.Validators;

public class CreateWebhookCommandValidator : AbstractValidator<CreateWebhookCommand>
{
    public CreateWebhookCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format.");
        RuleFor(x => x.Secret)
            .NotEmpty()
            .MinimumLength(16)
            .WithMessage("Webhook secret must be at least 16 characters.");
        RuleFor(x => x.EventTypes)
            .NotEmpty()
            .WithMessage("At least one event type is required.");
    }
}
