using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Commands.SaveStaffProfile;

// ─── Command ─────────────────────────────────────────────────────────────────

public record SaveStaffProfileCommand(
    Guid UserId,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    DateTimeOffset DateOfBirth,
    string Nationality,
    string MaritalStatus,
    string EmergencyContact,
    string BankAccountNo,
    string BankId,
    string NextOfKin
) : IRequest<Result<StaffProfile>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class SaveStaffProfileCommandValidator : AbstractValidator<SaveStaffProfileCommand>
{
    public SaveStaffProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.EmploymentStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaritalStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EmergencyContact).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BankAccountNo).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BankId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NextOfKin).NotEmpty().MaximumLength(200);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class SaveStaffProfileCommandHandler : IRequestHandler<SaveStaffProfileCommand, Result<StaffProfile>>
{
    private readonly IApplicationDbContext _context;

    public SaveStaffProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffProfile>> Handle(SaveStaffProfileCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<StaffProfile>.Failure("Associated User account not found.");

        var staff = await _context.StaffProfiles.FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);
        bool isNew = false;
        if (staff == null)
        {
            staff = new StaffProfile { Id = Guid.NewGuid(), UserId = request.UserId };
            isNew = true;
        }

        staff.HireDate = request.HireDate;
        staff.EmploymentStatus = request.EmploymentStatus;
        staff.DateOfBirth = request.DateOfBirth;
        staff.Nationality = request.Nationality;
        staff.MaritalStatus = request.MaritalStatus;
        staff.EmergencyContact = request.EmergencyContact;
        staff.BankAccountNo = request.BankAccountNo;
        staff.BankId = request.BankId;
        staff.NextOfKin = request.NextOfKin;

        if (isNew)
            _context.StaffProfiles.Add(staff);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<StaffProfile>.Success(staff);
    }
}
