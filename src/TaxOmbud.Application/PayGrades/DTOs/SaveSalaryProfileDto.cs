using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.PayGrades.DTOs;

public record SaveSalaryProfileCommand(
    Guid UserId,
    decimal Basic,
    string? Allowances,
    string? Deductions,
    DateTimeOffset EffectiveFrom
) ;

public record SavedSalaryProfileResponse(
    Guid Id,
    Guid UserId,
    decimal Basic,
    DateTimeOffset EffectiveFrom
);