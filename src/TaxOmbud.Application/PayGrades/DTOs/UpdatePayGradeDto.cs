using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.PayGrades.DTOs;

public record UpdatePayGradeCommand(
    Guid Id,
    string Name,
    int Level,
    string BasicSalaryBand,
    string Currency,
    decimal MinSalary,
    decimal MaxSalary,
    string? Description
) ;

public record UpdatePayGradeRequest(
    string Name,
    int Level,
    string BasicSalaryBand,
    string Currency,
    decimal MinSalary,
    decimal MaxSalary,
    string? Description
);
