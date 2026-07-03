using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Roles.DTOs;

public record CreateRoleCommand(string Name, string? Description);

public record CreateRoleResponse(Guid Id, string Name, string? Description);
