using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Common;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record GetStaffQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20
) ;

public record StaffListDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string? JobTitle,
    string DepartmentName,
    DateTimeOffset HireDate,
    string EmploymentStatus,
    string MaritalStatus
);
