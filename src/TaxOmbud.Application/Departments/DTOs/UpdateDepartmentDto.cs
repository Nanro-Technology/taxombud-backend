using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Departments.DTOs;

public record UpdateDepartmentCommand(Guid Id, string Name, string RoutingMode, string? Description, Guid? HeadUserId) ;

public record UpdateDepartmentRequest(string Name, string RoutingMode, string? Description, Guid? HeadUserId);