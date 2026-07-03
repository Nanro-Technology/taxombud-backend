using TaxOmbud.Application.SystemSettings.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISystemSettingsService
{
    Task<Response<object?>> ToggleE2eeAsync(ToggleE2eeCommand request, CancellationToken cancellationToken = default);
    Task<Response<E2eeStatusDto>> GetE2eeStatusAsync(GetE2eeStatusQuery request, CancellationToken cancellationToken = default);
}
