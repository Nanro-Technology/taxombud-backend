using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Officers;

namespace TaxOmbud.Application.Features.Officers.Commands.CreateOfficerProfile;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateOfficerProfileCommand(
    Guid UserId,
    int MaxCaseload,
    string? EmployeeNumber,
    string? Specialisation
) : IRequest<Result<CreatedOfficerResponse>>;

public record CreatedOfficerResponse(
    Guid Id,
    Guid UserId,
    int MaxCaseload,
    string? EmployeeNumber,
    string? Specialisation
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreateOfficerProfileCommandValidator : AbstractValidator<CreateOfficerProfileCommand>
{
    public CreateOfficerProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MaxCaseload).GreaterThanOrEqualTo(0).WithMessage("Max caseload must be greater than or equal to zero.");
        RuleFor(x => x.EmployeeNumber).MaximumLength(50);
        RuleFor(x => x.Specialisation).MaximumLength(200);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateOfficerProfileCommandHandler : IRequestHandler<CreateOfficerProfileCommand, Result<CreatedOfficerResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateOfficerProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreatedOfficerResponse>> Handle(CreateOfficerProfileCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<CreatedOfficerResponse>.Failure("User not found.");

        var alreadyExists = await _context.OfficerProfiles.AnyAsync(o => o.UserId == request.UserId, cancellationToken);
        if (alreadyExists)
            return Result<CreatedOfficerResponse>.Failure("An officer profile already exists for this user.");

        var profile = OfficerProfile.Create(request.UserId);
        profile.MaxCaseload = request.MaxCaseload;
        profile.EmployeeNumber = request.EmployeeNumber;
        profile.Specialisation = request.Specialisation;

        _context.OfficerProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreatedOfficerResponse(
            profile.Id,
            profile.UserId,
            profile.MaxCaseload,
            profile.EmployeeNumber,
            profile.Specialisation
        );

        return Result<CreatedOfficerResponse>.Success(response);
    }
}
