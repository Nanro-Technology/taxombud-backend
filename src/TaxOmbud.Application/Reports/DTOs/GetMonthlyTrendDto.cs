using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Reports.DTOs;

public record GetMonthlyTrendQuery(int? Year) ;

public record MonthlyTrendResponseDto(
    int Year,
    IEnumerable<MonthlyTrendDto> Monthly
);

public record MonthlyTrendDto(
    int Month,
    int Count
);