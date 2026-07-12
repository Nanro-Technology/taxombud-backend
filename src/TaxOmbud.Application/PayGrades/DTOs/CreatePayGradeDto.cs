using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.PayGrades.DTOs;

public record CreatePayGradeCommand(
    string Name,
    int Level,
    string BasicSalaryBand,
    string Currency,
    decimal MinSalary,
    decimal MaxSalary,
    string? Description
) ;

public record CreatedPayGradeResponse(
    Guid Id,
    string Name,
    int Level
);

public record CreatePayGradeRequest(
    string Name,
    int Level,
    string BasicSalaryBand,
    string Currency,
    decimal MinSalary,
    decimal MaxSalary,
    string? Description
);
