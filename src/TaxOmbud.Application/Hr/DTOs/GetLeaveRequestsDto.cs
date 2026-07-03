using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record GetLeaveRequestsQuery(Guid? UserId, string? Status) ;

public record LeaveRequestDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string LeaveType,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Days,
    string Status,
    string? SupervisorNote
);
