using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Reports.DTOs;

public record CreateScheduledReportCommand(
    string ReportName,
    string CronExpression,
    string[] Recipients,
    string? Format
) ;

public record CreatedScheduledReportResponse(
    Guid Id,
    string ReportName,
    string CronExpression,
    string Format
);

public record CreateScheduledReportRequest(string ReportName, string CronExpression, string[] Recipients, string? Format);
