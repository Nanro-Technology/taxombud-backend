using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record GetStaffByIdQuery(Guid Id) ;

public record StaffDetailDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    StaffDepartmentDto? Department,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    DateTimeOffset DateOfBirth,
    string Nationality,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string BankAccountNo,
    string BankId,
    string? NextOfKinName,
    string? NextOfKinPhone
);

public record StaffDepartmentDto(Guid Id, string Name);