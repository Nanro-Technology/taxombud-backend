using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseMilestonesQuery(Guid CaseId) ;

public record CaseMilestoneDto(
    Guid Id,
    Guid CaseId,
    string Title,
    string? Description,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedAt,
    bool IsCompleted
);