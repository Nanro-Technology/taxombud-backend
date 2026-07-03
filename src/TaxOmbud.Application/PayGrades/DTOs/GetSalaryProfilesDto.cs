using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.PayGrades.DTOs;

public record GetSalaryProfilesQuery(Guid? UserId) ;

public record SalaryProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    decimal Basic,
    string? Allowances,
    string? Deductions,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset CreatedAt
);
