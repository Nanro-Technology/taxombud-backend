using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Webhooks.Commands.UpdateWebhook;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateWebhookCommand(
    Guid Id,
    string Url,
    string[] EventTypes,
    bool IsActive
) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateWebhookCommandHandler : IRequestHandler<UpdateWebhookCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateWebhookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhook = await _context.WebhookSubscriptions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (webhook == null)
            return Result<Unit>.NotFound("Webhook subscription not found.");

        webhook.Url = request.Url;
        webhook.EventTypes = string.Join(",", request.EventTypes);
        webhook.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
