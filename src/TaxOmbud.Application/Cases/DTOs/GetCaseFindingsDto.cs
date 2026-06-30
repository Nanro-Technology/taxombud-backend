using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseFindingsQuery(Guid CaseId) ;

public record CaseFindingDto(
    Guid Id,
    Guid CaseId,
    string Description,
    DateTimeOffset CreatedAt,
    Guid? CreatedBy
);