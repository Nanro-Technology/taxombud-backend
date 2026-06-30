using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record CreateOfficerProfileCommand(
    Guid UserId,
    int MaxCaseload,
    string? EmployeeNumber,
    string? Specialisation
) ;

public record CreatedOfficerResponse(
    Guid Id,
    Guid UserId,
    int MaxCaseload,
    string? EmployeeNumber,
    string? Specialisation
);

public record CreateOfficerProfileRequest(Guid UserId, int MaxCaseload, string? EmployeeNumber, string? Specialisation);