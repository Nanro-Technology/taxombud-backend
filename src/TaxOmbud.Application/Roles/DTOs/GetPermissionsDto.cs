using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record GetPermissionsQuery() ;

public record PermissionDetailDto(string Code, string Action, string Entity, string? Description);