using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.SystemSettings.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISystemSettingsService
{
    Task<Response<object?>> ToggleE2eeAsync(ToggleE2eeCommand request, CancellationToken cancellationToken = default);
    Task<Response<E2eeStatusDto>> GetE2eeStatusAsync(GetE2eeStatusQuery request, CancellationToken cancellationToken = default);
}