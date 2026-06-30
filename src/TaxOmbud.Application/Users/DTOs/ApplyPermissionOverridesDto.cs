using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record ApplyPermissionOverridesCommand(Guid Id, PermissionOverrideDto[] Overrides) ;

public record PermissionOverrideDto(string PermissionCode, string Mode);

public record PermissionOverridesRequest(PermissionOverrideDto[] Overrides);