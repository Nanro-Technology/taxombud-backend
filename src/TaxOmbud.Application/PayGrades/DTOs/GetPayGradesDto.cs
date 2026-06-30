using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.PayGrades.DTOs;

public record GetPayGradesQuery() ;

public record PayGradeDto(
    Guid Id,
    string Name,
    int Level,
    string BasicSalaryBand,
    DateTimeOffset CreatedAt
);