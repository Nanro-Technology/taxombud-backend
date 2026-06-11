using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Features.Webhooks.Commands.CreateWebhook;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateWebhookCommand(
    string Url,
    string Secret,
    string[] EventTypes
) : IRequest<Result<CreatedWebhookResponse>>;

public record CreatedWebhookResponse(
    Guid Id,
    string Url,
    string EventTypes,
    bool IsActive
);

// ─── Validator ────────────────────────────────────────────────────────────────

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateWebhookCommandHandler : IRequestHandler<CreateWebhookCommand, Result<CreatedWebhookResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateWebhookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreatedWebhookResponse>> Handle(CreateWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhook = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            Url = request.Url,
            Secret = request.Secret,
            EventTypes = string.Join(",", request.EventTypes),
            IsActive = true
        };

        _context.WebhookSubscriptions.Add(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreatedWebhookResponse(webhook.Id, webhook.Url, webhook.EventTypes, webhook.IsActive);
        return Result<CreatedWebhookResponse>.Success(response);
    }
}
