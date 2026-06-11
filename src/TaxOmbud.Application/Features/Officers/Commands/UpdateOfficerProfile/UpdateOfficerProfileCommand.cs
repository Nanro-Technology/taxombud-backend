using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Officers.Commands.UpdateOfficerProfile;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateOfficerProfileCommand(
    Guid Id,
    int MaxCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation
) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateOfficerProfileCommandValidator : AbstractValidator<UpdateOfficerProfileCommand>
{
    public UpdateOfficerProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MaxCaseload).GreaterThanOrEqualTo(0).WithMessage("Max caseload must be greater than or equal to zero.");
        RuleFor(x => x.EmployeeNumber).MaximumLength(50);
        RuleFor(x => x.Specialisation).MaximumLength(200);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateOfficerProfileCommandHandler : IRequestHandler<UpdateOfficerProfileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateOfficerProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateOfficerProfileCommand request, CancellationToken cancellationToken)
    {
        var officer = await _context.OfficerProfiles.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (officer == null)
            return Result<Unit>.NotFound("Officer profile not found.");

        officer.MaxCaseload = request.MaxCaseload;
        officer.IsAvailable = request.IsAvailable;
        officer.EmployeeNumber = request.EmployeeNumber;
        officer.Specialisation = request.Specialisation;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
