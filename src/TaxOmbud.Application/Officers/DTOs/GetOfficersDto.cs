using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Officers.DTOs;

public record GetOfficersQuery(
    Guid? DepartmentId,
    string? Search,
    int Page = 1,
    int PageSize = 20
) ;

public record OfficerListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    OfficerDepartmentDto? Department,
    int MaxCaseload,
    int CurrentCaseload,
    bool IsAvailable,
    string? EmployeeNumber,
    string? Specialisation,
    DateTimeOffset CreatedAt
);
