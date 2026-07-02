using TaxOmbud.Application.Geo.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IGeoService
{
    Task<Response<IReadOnlyList<CountryDto>>> GetCountriesAsync(GetCountriesQuery request, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<StateDto>>> GetStatesAsync(GetStatesQuery request, CancellationToken cancellationToken = default);
}
