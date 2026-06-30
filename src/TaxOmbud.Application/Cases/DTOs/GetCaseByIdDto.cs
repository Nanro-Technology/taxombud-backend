using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseByIdQuery(Guid Id) ;

public record CaseDetailDto(
    Guid Id,
    string CaseNumber,
    string Subject,
    string? Summary,
    string Priority,
    string Status,
    string CurrentStage,
    CaseOfficerDto? AssignedOfficer,
    CaseDepartmentDto? Department,
    DateTimeOffset? DueDate,
    DateTimeOffset? ClosedAt,
    string? Outcome,
    string? FindingsSummary,
    CaseComplaintDto Complaint,
    IEnumerable<FindingDto> Findings,
    IEnumerable<RecommendationDto> Recommendations,
    IEnumerable<MilestoneDto> Milestones,
    IEnumerable<StatusHistoryDto> StatusHistory
);

public record CaseOfficerDto(Guid Id, string FullName, string Email);
public record CaseDepartmentDto(Guid Id, string Name);
public record CaseComplaintDto(Guid Id, string ReferenceNumber, ComplaintTaxpayerDto? Taxpayer, string TaxType, string TaxPeriod, string ComplaintCategory);
public record ComplaintTaxpayerDto(Guid Id, string FullName, string Email);
public record FindingDto(Guid Id, string Description, DateTimeOffset CreatedAt);
public record RecommendationDto(Guid Id, string RecommendationText, Guid ApprovedByUserId, DateTimeOffset CreatedAt);
public record MilestoneDto(Guid Id, string Title, string? Description, DateTimeOffset CreatedAt);
public record StatusHistoryDto(Guid Id, string PreviousStatus, string NewStatus, Guid ChangedByUserId, DateTimeOffset CreatedAt);