using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetComplaintByIdQuery(Guid Id) ;

public record ComplaintDetailDto(
    Guid Id,
    string ReferenceNumber,
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Status,
    string CurrentStage,
    string Priority,
    bool RequiresApprovalToClose,
    DateTimeOffset? ClosedAt,
    string? ClosureReason,
    string? WithdrawalReason,
    TaxpayerSummary Taxpayer,
    OfficerSummary? AssignedOfficer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record TaxpayerSummary(Guid Id, string FullName, string? Email, string? Phone);
public record OfficerSummary(Guid Id, string FullName, string? Email);
