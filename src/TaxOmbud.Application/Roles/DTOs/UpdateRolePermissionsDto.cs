using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record UpdateRolePermissionsCommand(Guid RoleId, Guid[] PermissionIds);

public record UpdateRolePermissionsRequest(Guid[] PermissionIds);