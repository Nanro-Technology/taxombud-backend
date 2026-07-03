using FluentValidation;
using TaxOmbud.Application.Webhooks.DTOs;

namespace TaxOmbud.Application.Webhooks.Validators;

public class DeleteWebhookCommandValidator : AbstractValidator<DeleteWebhookCommand>
{
    public DeleteWebhookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
