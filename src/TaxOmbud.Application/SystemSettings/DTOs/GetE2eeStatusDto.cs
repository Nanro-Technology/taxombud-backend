using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.SystemSettings.DTOs;

public record GetE2eeStatusQuery() ;

public record E2eeStatusDto(bool IsEnabled);
