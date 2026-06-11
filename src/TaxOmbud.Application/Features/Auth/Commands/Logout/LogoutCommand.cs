using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Auth.Commands.Logout;

// ─── Command ─────────────────────────────────────────────────────────────────

public record LogoutCommand(string RefreshToken) : IRequest<Result<object?>>;

// ─── Validator ───────────────────────────────────────────────────────────────

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token is not null)
        {
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Always return success (don't leak token existence)
        return Result<object?>.Success(null);
    }
}
