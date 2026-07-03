using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Departments.DTOs;

public record CreateDepartmentCommand(string Name, string RoutingMode, string? Description, Guid? HeadUserId) ;

public record CreateDepartmentResponse(Guid Id, string Name, string RoutingMode, string? Description);
