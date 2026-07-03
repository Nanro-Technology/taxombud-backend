using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Reports.DTOs;

public record GetScheduledReportsQuery() ;

public record ScheduledReportDto(
    Guid Id,
    string ReportName,
    string CronExpression,
    string Recipients,
    string Format,
    bool IsActive,
    DateTimeOffset? LastRunAt,
    DateTimeOffset CreatedAt
);
