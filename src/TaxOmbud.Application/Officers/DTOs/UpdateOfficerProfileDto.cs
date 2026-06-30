using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record UpdateOfficerProfileCommand(
    Guid Id,
    int MaxCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation
) ;