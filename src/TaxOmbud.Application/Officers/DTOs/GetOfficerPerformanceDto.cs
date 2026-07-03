using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record GetOfficerPerformanceQuery(Guid OfficerId);

public record OfficerPerformanceDto(
    Guid OfficerId,
    string OfficerName,
    int CasesHandled,
    double AverageResolutionDays
);
