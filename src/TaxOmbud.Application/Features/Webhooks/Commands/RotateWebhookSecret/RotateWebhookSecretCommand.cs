using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Webhooks.Commands.RotateWebhookSecret;

// ─── Command ─────────────────────────────────────────────────────────────────

public record RotateWebhookSecretCommand(Guid Id, string NewSecret) : IRequest<Result<RotateSecretResponseDto>>;

public record RotateSecretResponseDto(string Message);

// ─── Validator ────────────────────────────────────────────────────────────────

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

// ─── Handler ─────────────────────────────────────────────────────────────────

public class RotateWebhookSecretCommandHandler : IRequestHandler<RotateWebhookSecretCommand, Result<RotateSecretResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public RotateWebhookSecretCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RotateSecretResponseDto>> Handle(RotateWebhookSecretCommand request, CancellationToken cancellationToken)
    {
        var webhook = await _context.WebhookSubscriptions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (webhook == null)
            return Result<RotateSecretResponseDto>.NotFound("Webhook subscription not found.");

        webhook.Secret = request.NewSecret;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RotateSecretResponseDto>.Success(new RotateSecretResponseDto("Secret rotated successfully."));
    }
}
