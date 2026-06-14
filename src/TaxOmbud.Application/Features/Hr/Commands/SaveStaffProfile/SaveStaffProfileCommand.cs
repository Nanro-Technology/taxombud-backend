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
    string? EmployeeCode,
    string? Title,
    Guid? SupervisorId,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    DateTimeOffset DateOfBirth,
    string Nationality,
    string MaritalStatus,
    string? EducationLevel,
    string? EducationDetails,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string BankAccountNo,
    string BankId,
    string? NextOfKinName,
    string? NextOfKinRelationship,
    string? NextOfKinPhone,
    string? NextOfKinAddress
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
        RuleFor(x => x.BankAccountNo).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BankId).NotEmpty().MaximumLength(50);
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

        staff.EmployeeCode = request.EmployeeCode;
        staff.Title = request.Title;
        staff.SupervisorId = request.SupervisorId;
        staff.HireDate = request.HireDate;
        staff.EmploymentStatus = request.EmploymentStatus;
        staff.DateOfBirth = request.DateOfBirth;
        staff.Nationality = request.Nationality;
        staff.MaritalStatus = request.MaritalStatus;
        staff.EducationLevel = request.EducationLevel;
        staff.EducationDetails = request.EducationDetails;
        staff.AddressLine1 = request.AddressLine1;
        staff.AddressLine2 = request.AddressLine2;
        staff.City = request.City;
        staff.State = request.State;
        staff.Country = request.Country;
        staff.EmergencyContactName = request.EmergencyContactName;
        staff.EmergencyContactPhone = request.EmergencyContactPhone;
        staff.BankAccountNo = request.BankAccountNo;
        staff.BankId = request.BankId;
        staff.NextOfKinName = request.NextOfKinName;
        staff.NextOfKinRelationship = request.NextOfKinRelationship;
        staff.NextOfKinPhone = request.NextOfKinPhone;
        staff.NextOfKinAddress = request.NextOfKinAddress;

        if (isNew)
            _context.StaffProfiles.Add(staff);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<StaffProfile>.Success(staff);
    }
}
