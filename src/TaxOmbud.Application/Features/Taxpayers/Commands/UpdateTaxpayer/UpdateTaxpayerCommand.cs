using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Taxpayers.Commands.UpdateTaxpayer;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateTaxpayerCommand(
    Guid TaxpayerId,
    string FirstName,
    string LastName,
    string Phone,
    string? TinNumber,
    string? Nin,
    string? Bvn,
    string? Gender,
    DateTimeOffset? DateOfBirth,
    string? CompanyName,
    string? RcNumber,
    string? Address,
    string? City,
    string? State
) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateTaxpayerCommandValidator : AbstractValidator<UpdateTaxpayerCommand>
{
    public UpdateTaxpayerCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateTaxpayerCommandHandler : IRequestHandler<UpdateTaxpayerCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateTaxpayerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateTaxpayerCommand request, CancellationToken cancellationToken)
    {
        var taxpayer = await _context.TaxpayerProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TaxpayerId, cancellationToken);

        if (taxpayer == null)
            return Result<Unit>.NotFound("Taxpayer profile not found.");

        taxpayer.User!.UpdateProfile(request.FirstName, request.LastName, request.Phone, taxpayer.User.JobTitle);
        taxpayer.TinNumber = request.TinNumber;
        taxpayer.Nin = request.Nin;
        taxpayer.Bvn = request.Bvn;
        taxpayer.Gender = request.Gender;
        taxpayer.DateOfBirth = request.DateOfBirth;
        taxpayer.CompanyName = request.CompanyName;
        taxpayer.RcNumber = request.RcNumber;
        taxpayer.Address = request.Address;
        taxpayer.City = request.City;
        taxpayer.State = request.State;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
