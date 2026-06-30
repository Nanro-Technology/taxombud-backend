using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCasesQuery(
    string? Search,
    string? Stage,
    string? Status,
    int Page = 1,
    int PageSize = 20
) ;

public record CaseListDto(
    Guid Id,
    string CaseNumber,
    Guid ComplaintId,
    string ComplaintRef,
    string TaxpayerName,
    string Subject,
    string Priority,
    string Status,
    string CurrentStage,
    string AssignedOfficerName,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt
);