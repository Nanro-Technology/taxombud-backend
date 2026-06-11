using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.ValueObjects;

namespace TaxOmbud.Application.Features.Auth.Commands.Register;

// ─── Command ─────────────────────────────────────────────────────────────────

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(Guid UserId, string Email, string FullName);

// ─── Validator ────────────────────────────────────────────────────────────────

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*()\-_=+]").WithMessage("Password must contain at least one special character.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users
            .AnyAsync(u => u.Email == emailNormalized, cancellationToken);

        if (exists)
            return Result<RegisterResponse>.Conflict($"An account with email '{request.Email}' already exists.");

        // Resolve the default Taxpayer role
        var taxpayerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Taxpayer", cancellationToken);

        // Create user
        var emailVo = new Email(request.Email);
        var user = User.Create(request.FirstName, request.LastName, emailVo, request.PhoneNumber);
        user.SetPasswordHash(_passwordHasher.Hash(request.Password));

        if (taxpayerRole != null)
            user.AddRole(taxpayerRole.Id);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id, user.Email, user.FullName));
    }
}
