using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.System.Commands.ToggleFeatureFlag;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ToggleFeatureFlagCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class ToggleFeatureFlagCommandValidator : AbstractValidator<ToggleFeatureFlagCommand>
{
    public ToggleFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ToggleFeatureFlagCommandHandler : IRequestHandler<ToggleFeatureFlagCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ToggleFeatureFlagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ToggleFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (flag == null)
            return Result<Unit>.NotFound("Feature flag not found.");

        flag.IsEnabled = !flag.IsEnabled;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
