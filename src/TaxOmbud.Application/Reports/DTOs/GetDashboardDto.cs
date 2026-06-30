using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Reports.DTOs;

public record GetDashboardQuery() ;

public record DashboardStatsDto(
    ComplaintsStatsDto Complaints,
    CasesStatsDto Cases,
    AppealsStatsDto Appeals,
    StaffStatsDto Staff,
    double AvgResolutionDays
);

public record ComplaintsStatsDto(int Total, int Open, int Closed);
public record CasesStatsDto(int Total, int Open, int Closed);
public record AppealsStatsDto(int Total, int Pending);
public record StaffStatsDto(int Officers, int Taxpayers);