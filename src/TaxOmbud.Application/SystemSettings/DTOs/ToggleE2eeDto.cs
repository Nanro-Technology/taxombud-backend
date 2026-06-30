using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.SystemSettings.DTOs;

public record ToggleE2eeCommand(bool Enable) ;


public record ToggleE2eeRequest(bool Enable);