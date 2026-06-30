using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record GetOfficerCaseloadsQuery(Guid OfficerId, bool? ActiveOnly) ;

public record OfficerCaseloadsDto(
    Guid OfficerId,
    IEnumerable<CaseloadDto> Caseloads
);

public record CaseloadDto(
    Guid Id,
    Guid CaseId,
    bool IsActive,
    DateTimeOffset AssignedAt,
    DateTimeOffset? CompletedAt
);