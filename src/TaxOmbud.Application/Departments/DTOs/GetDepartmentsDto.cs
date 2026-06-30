using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Departments.DTOs;

public record GetDepartmentsQuery() ;

public record DepartmentDto(
    Guid Id,
    string Name,
    string RoutingMode,
    string? Description,
    HeadUserDto? HeadUser
);

public record HeadUserDto(Guid Id, string FullName, string Email);