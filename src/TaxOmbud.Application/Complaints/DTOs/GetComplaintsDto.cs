using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetComplaintsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? TaxType = null,
    Guid? TaxpayerId = null,
    Guid? AssignedOfficerId = null,
    string? Search = null
) ;

public record ComplaintSummaryDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string Status,
    string CurrentStage,
    string Priority,
    Guid TaxpayerId,
    string? TaxpayerName,
    Guid? AssignedOfficerId,
    string? AssignedOfficerName,
    DateTimeOffset CreatedAt
);