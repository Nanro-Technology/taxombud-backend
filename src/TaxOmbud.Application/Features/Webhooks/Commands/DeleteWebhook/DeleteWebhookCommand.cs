using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Webhooks.Commands.DeleteWebhook;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeleteWebhookCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class DeleteWebhookCommandValidator : AbstractValidator<DeleteWebhookCommand>
{
    public DeleteWebhookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeleteWebhookCommandHandler : IRequestHandler<DeleteWebhookCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteWebhookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhook = await _context.WebhookSubscriptions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (webhook == null)
            return Result<Unit>.NotFound("Webhook subscription not found.");

        _context.WebhookSubscriptions.Remove(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
