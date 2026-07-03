using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record GetOfficerByIdQuery(Guid Id) ;

public record OfficerDetailDto(
    Guid Id,
    Guid UserId,
    string? FullName,
    string? Email,
    string? Phone,
    string? JobTitle,
    OfficerDepartmentDto? Department,
    int MaxCaseload,
    int CurrentCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation,
    int ActiveCaseloads,
    DateTimeOffset CreatedAt
);

public record OfficerDepartmentDto(Guid Id, string Name);
