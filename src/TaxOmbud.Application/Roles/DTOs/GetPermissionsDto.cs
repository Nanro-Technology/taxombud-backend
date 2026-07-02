using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record GetPermissionsQuery() ;

/// <summary>Permission DTO: Module (e.g. "Complaints") × Action (e.g. "View").</summary>
public record PermissionDetailDto(Guid Id, string Module, string Action);