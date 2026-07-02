using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record GetRolesQuery() ;

public record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole, bool IsActive);