using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.PayGrades.Commands.SaveSalaryProfile;

// ─── Command ─────────────────────────────────────────────────────────────────

public record SaveSalaryProfileCommand(
    Guid UserId,
    decimal Basic,
    string? Allowances,
    string? Deductions,
    DateTimeOffset EffectiveFrom
) : IRequest<Result<SavedSalaryProfileResponse>>;

public record SavedSalaryProfileResponse(
    Guid Id,
    Guid UserId,
    decimal Basic,
    DateTimeOffset EffectiveFrom
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class SaveSalaryProfileCommandValidator : AbstractValidator<SaveSalaryProfileCommand>
{
    public SaveSalaryProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Basic).GreaterThanOrEqualTo(0).WithMessage("Basic salary must be greater than or equal to zero.");
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class SaveSalaryProfileCommandHandler : IRequestHandler<SaveSalaryProfileCommand, Result<SavedSalaryProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public SaveSalaryProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SavedSalaryProfileResponse>> Handle(SaveSalaryProfileCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<SavedSalaryProfileResponse>.Failure("User not found.");

        // Close any existing active profile
        var existing = await _context.SalaryProfiles
            .Where(s => s.UserId == request.UserId && s.EffectiveTo == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
            existing.EffectiveTo = request.EffectiveFrom.AddDays(-1);

        var profile = new SalaryProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Basic = request.Basic,
            Allowances = request.Allowances,
            Deductions = request.Deductions,
            EffectiveFrom = request.EffectiveFrom
        };

        _context.SalaryProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new SavedSalaryProfileResponse(profile.Id, profile.UserId, profile.Basic, profile.EffectiveFrom);
        return Result<SavedSalaryProfileResponse>.Success(response);
    }
}
