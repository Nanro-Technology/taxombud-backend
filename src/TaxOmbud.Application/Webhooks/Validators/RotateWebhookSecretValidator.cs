using FluentValidation;
using TaxOmbud.Application.Webhooks.DTOs;

namespace TaxOmbud.Application.Webhooks.Validators;

public class RotateWebhookSecretCommandValidator : AbstractValidator<RotateWebhookSecretCommand>
{
    public RotateWebhookSecretCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewSecret)
            .NotEmpty()
            .MinimumLength(16)
            .WithMessage("New secret must be at least 16 characters.");
    }
}
