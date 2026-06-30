using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

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
) ;