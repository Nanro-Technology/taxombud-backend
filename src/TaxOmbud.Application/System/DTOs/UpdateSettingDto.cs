using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.System.DTOs;

public record UpdateSettingCommand(string Key, string Value, string? Description) ;